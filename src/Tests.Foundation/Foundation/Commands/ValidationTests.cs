using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;

namespace IOKode.OpinionatedFramework.Tests.Foundation.Commands;

public class ValidationTests
{
    [Fact]
    public async Task CommandWithValidData_HasNoErrors()
    {
        // Arrange
        var command = new BookCommand
        {
            FlightNumber = "IB3456",
            Passenger = ValidPassenger(),
            Seats = [new SeatInput { Row = 12 }],
        };

        // Act
        var errors = await command.ValidateAsync(CancellationToken.None);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public async Task NestedInputs_ErrorHasThePathOfTheirProperties()
    {
        // Arrange
        var command = new BookCommand
        {
            FlightNumber = "IB3456",
            Passenger = new PassengerInput { Name = "Ana", Document = new DocumentInput { Number = "" } },
            Seats = [],
        };

        // Act
        var errors = await command.ValidateAsync(CancellationToken.None);

        // Assert
        var error = Assert.IsType<PathValidationError>(Assert.Single(errors));
        Assert.Equal("Passenger.Document.Number", error.Path.ToString());
        Assert.Equal(
            [new PropertyPathSegment("Passenger"), new PropertyPathSegment("Document"), new PropertyPathSegment("Number")],
            error.Path.Segments);
        Assert.Equal("A document number is required.", error.Message);
    }

    [Fact]
    public async Task CollectionOfInputs_ErrorHasTheIndexOfTheElementAndNullElementsAreSkipped()
    {
        // Arrange
        var command = new BookCommand
        {
            FlightNumber = "IB3456",
            Passenger = ValidPassenger(),
            Seats = [new SeatInput { Row = 1 }, null!, new SeatInput { Row = 0 }],
        };

        // Act
        var errors = await command.ValidateAsync(CancellationToken.None);

        // Assert
        var error = Assert.IsType<PathValidationError>(Assert.Single(errors));
        Assert.Equal("Seats[2].Row", error.Path.ToString());
        Assert.Equal(
            [new PropertyPathSegment("Seats"), new IndexPathSegment(2), new PropertyPathSegment("Row")],
            error.Path.Segments);
    }

    [Fact]
    public async Task DictionaryOfInputs_ErrorHasTheKeyOfTheValue()
    {
        // Arrange
        var command = new AssignSeatsCommand
        {
            SeatsByCode = new Dictionary<string, SeatInput?>
            {
                ["a.b"] = new() { Row = 0 },
                ["ok"] = new() { Row = 3 },
                ["empty"] = null,
            },
            SeatsById = new Dictionary<int, SeatInput> { [42] = new() { Row = 0 } },
        };

        // Act
        var errors = await command.ValidateAsync(CancellationToken.None);

        // Assert
        var paths = errors.Cast<PathValidationError>().Select(error => error.Path).ToArray();
        Assert.Equal(2, paths.Length);
        Assert.Equal("SeatsByCode[\"a.b\"].Row", paths[0].ToString());
        Assert.Equal(
            [new PropertyPathSegment("SeatsByCode"), new KeyPathSegment("a.b"), new PropertyPathSegment("Row")],
            paths[0].Segments);
        Assert.Equal("SeatsById[42].Row", paths[1].ToString());
        Assert.Equal(
            [new PropertyPathSegment("SeatsById"), new KeyPathSegment(42), new PropertyPathSegment("Row")],
            paths[1].Segments);
    }

    [Fact]
    public async Task NestedCollectionsAndDictionaries_AreGoneInto()
    {
        // Arrange
        var command = new ArrangeSeatsCommand
        {
            SeatRows = [[new SeatInput { Row = 1 }], [null!, new SeatInput { Row = 0 }]],
            SeatsByZone = new Dictionary<string, List<SeatInput>> { ["front"] = [new SeatInput { Row = 0 }] },
        };

        // Act
        var errors = await command.ValidateAsync(CancellationToken.None);

        // Assert
        var paths = errors.Cast<PathValidationError>().Select(error => error.Path).ToArray();
        Assert.Equal(2, paths.Length);
        Assert.Equal(
            [new PropertyPathSegment("SeatRows"), new IndexPathSegment(1), new IndexPathSegment(1), new PropertyPathSegment("Row")],
            paths[0].Segments);
        Assert.Equal("SeatRows[1][1].Row", paths[0].ToString());
        Assert.Equal(
            [new PropertyPathSegment("SeatsByZone"), new KeyPathSegment("front"), new IndexPathSegment(0), new PropertyPathSegment("Row")],
            paths[1].Segments);
        Assert.Equal("SeatsByZone[\"front\"][0].Row", paths[1].ToString());
    }

    [Fact]
    public async Task NullInput_IsSkipped()
    {
        // Arrange
        var command = new BookCommand
        {
            FlightNumber = "IB3456",
            Passenger = null!,
            Seats = null!,
        };

        // Act
        var errors = await command.ValidateAsync(CancellationToken.None);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public async Task UpdatableNotToBeUpdated_IsNotValidated()
    {
        // Arrange
        var command = new UpdateBookingCommand
        {
            Passenger = new Updatable<PassengerInput> { Value = InvalidPassenger(), ShouldUpdate = false },
            Seats = new Updatable<IReadOnlyList<SeatInput>> { Value = [new SeatInput { Row = 0 }], ShouldUpdate = false },
        };

        // Act
        var errors = await command.ValidateAsync(CancellationToken.None);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public async Task UpdatableToBeUpdated_IsValidatedWithThePathOfItsProperty()
    {
        // Arrange
        var command = new UpdateBookingCommand
        {
            Passenger = InvalidPassenger(),
            Seats = new Updatable<IReadOnlyList<SeatInput>>([new SeatInput { Row = 0 }]),
        };

        // Act
        var errors = await command.ValidateAsync(CancellationToken.None);

        // Assert
        Assert.Equal(["Passenger.Name", "Seats[0].Row"], errors.Cast<PathValidationError>().Select(error => error.Path.ToString()));
    }

    [Fact]
    public async Task UpdatableToBeUpdatedToNull_IsSkipped()
    {
        // Arrange
        var command = new UpdateBookingCommand
        {
            Passenger = new Updatable<PassengerInput>(null!),
            Seats = default,
        };

        // Act
        var errors = await command.ValidateAsync(CancellationToken.None);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public async Task OverrideThatDoesNotCallValidateInputs_ValidatesNoInput()
    {
        // Arrange
        var command = new OwnRulesOnlyCommand { Passenger = InvalidPassenger() };

        // Act
        var errors = await command.ValidateAsync(CancellationToken.None);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public async Task ObjectErrorOfAnInput_ReachesTheCommandWithThePathOfTheInput()
    {
        // Arrange
        var command = new ContactCommand { Contact = new ContactInput() };

        // Act
        var errors = await command.ValidateAsync(CancellationToken.None);

        // Assert
        var error = Assert.IsType<PathValidationError>(Assert.Single(errors));
        Assert.Equal("Contact", error.Path.ToString());
        Assert.Equal("An email or a phone is required.", error.Message);
    }

    [Fact]
    public async Task ObjectErrorOfTheCommand_StaysAnObjectError()
    {
        // Arrange
        var command = new DateRangeCommand { Start = new DateOnly(2026, 5, 2), End = new DateOnly(2026, 5, 1) };

        // Act
        var errors = await command.ValidateAsync(CancellationToken.None);

        // Assert
        var error = Assert.IsType<ObjectValidationError>(Assert.Single(errors));
        Assert.Equal("The end cannot be before the start.", error.Message);
    }

    [Fact]
    public async Task SamePath_CanHaveSeveralErrors()
    {
        // Arrange
        var command = new BookCommand
        {
            FlightNumber = "IB3456",
            Passenger = ValidPassenger(),
            Seats = [new SeatInput { Row = 120 }],
        };

        // Act
        var errors = await command.ValidateAsync(CancellationToken.None);

        // Assert
        Assert.Equal(
            ["Seats[0].Row: The row does not exist.", "Seats[0].Row: The row is not for sale."],
            errors.Select(error => error.ToString()));
    }

    [Fact]
    public async Task CommandWithResult_ValidatesItsInputs()
    {
        // Arrange
        var command = new RegisterPassengerCommand { Passenger = InvalidPassenger() };

        // Act
        var errors = await command.ValidateAsync(CancellationToken.None);

        // Assert
        var error = Assert.IsType<PathValidationError>(Assert.Single(errors));
        Assert.Equal("Passenger.Name", error.Path.ToString());
    }

    [Fact]
    public async Task ThrowIfInvalid_InvalidCommand_ThrowsWithAllTheErrors()
    {
        // Arrange
        var command = new BookCommand
        {
            FlightNumber = "IB3456",
            Passenger = InvalidPassenger(),
            Seats = [new SeatInput { Row = 0 }],
        };

        // Act
        var exception = await Assert.ThrowsAsync<InvalidInputException>(
            () => CommandValidation.ThrowIfInvalidAsync(command, CancellationToken.None));

        // Assert
        Assert.Same(command, exception.Command);
        Assert.Equal(["Passenger.Name", "Seats[0].Row"], exception.Errors.Cast<PathValidationError>().Select(error => error.Path.ToString()));
    }

    [Fact]
    public async Task ThrowIfInvalid_ValidCommand_DoesNotThrow()
    {
        // Arrange
        var command = new RegisterPassengerCommand { Passenger = ValidPassenger() };

        // Act & Assert
        await CommandValidation.ThrowIfInvalidAsync(command, CancellationToken.None);
    }

    [Fact]
    public async Task ThrowIfInvalid_ValidationReturnsNull_Throws()
    {
        // Arrange
        var command = new NullValidationCommand();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidValidationResultException>(
            () => CommandValidation.ThrowIfInvalidAsync(command, CancellationToken.None));
        Assert.Equal(typeof(NullValidationCommand), exception.ValidatedType);
    }

    [Fact]
    public async Task InputValidationReturnsNull_Throws()
    {
        // Arrange
        var command = new NullInputValidationCommand { Input = new NullValidationInput() };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidValidationResultException>(
            () => command.ValidateAsync(CancellationToken.None));
        Assert.Equal(typeof(NullValidationInput), exception.ValidatedType);
    }

    [Fact]
    public async Task DictionaryOfInputsWithANullKey_Throws()
    {
        // Arrange
        var command = new NullKeySeatsCommand { Seats = new NullKeySeats() };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NullDictionaryKeyException>(
            () => command.ValidateAsync(CancellationToken.None));
        Assert.Equal(typeof(NullKeySeats), exception.DictionaryType);
    }

    [Fact]
    public void KeyWhoseToStringReturnsNull_Throws()
    {
        // Act & Assert
        var exception = Assert.Throws<KeyToStringReturnedNullException>(() => new KeyPathSegment(new NullTextKey()));
        Assert.Equal(typeof(NullTextKey), exception.KeyType);
    }

    [Fact]
    public void PathText_EscapesQuotesAndBackslashesOfAKey()
    {
        // Arrange
        var path = new ValidationPath(new PropertyPathSegment("Map"), new KeyPathSegment("say \"hi\" \\ bye"));

        // Act
        var text = path.ToString();

        // Assert
        Assert.Equal("Map[\"say \\\"hi\\\" \\\\ bye\"]", text);
    }

    [Fact]
    public void Path_WithoutSegments_Throws()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new ValidationPath());
    }

    private static PassengerInput ValidPassenger()
    {
        return new PassengerInput { Name = "Ana", Document = new DocumentInput { Number = "12345678Z" } };
    }

    private static PassengerInput InvalidPassenger()
    {
        return new PassengerInput { Name = " ", Document = new DocumentInput { Number = "12345678Z" } };
    }

    private class DocumentInput : Input
    {
        public required string Number { get; init; }

        public override async Task<ValidationErrors> ValidateAsync(CancellationToken cancellationToken)
        {
            var errors = await ValidateInputsAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(Number))
            {
                errors.Add(nameof(Number), "A document number is required.");
            }

            return errors;
        }
    }

    private class PassengerInput : Input
    {
        public required string Name { get; init; }
        public required DocumentInput Document { get; init; }

        public override async Task<ValidationErrors> ValidateAsync(CancellationToken cancellationToken)
        {
            var errors = await ValidateInputsAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(Name))
            {
                errors.Add(nameof(Name), "A name is required.");
            }

            return errors;
        }
    }

    private class SeatInput : Input
    {
        public required int Row { get; init; }

        public override Task<ValidationErrors> ValidateAsync(CancellationToken cancellationToken)
        {
            var errors = new ValidationErrors();
            if (Row is < 1 or > 60)
            {
                errors.Add(nameof(Row), "The row does not exist.");
            }

            if (Row > 100)
            {
                errors.Add(nameof(Row), "The row is not for sale.");
            }

            return Task.FromResult(errors);
        }
    }

    private class ContactInput : Input
    {
        public string? Email { get; init; }
        public string? Phone { get; init; }

        public override Task<ValidationErrors> ValidateAsync(CancellationToken cancellationToken)
        {
            var errors = new ValidationErrors();
            if (Email is null && Phone is null)
            {
                errors.AddForObject("An email or a phone is required.");
            }

            return Task.FromResult(errors);
        }
    }

    private class BookCommand : Command
    {
        public required string FlightNumber { get; init; }
        public required PassengerInput Passenger { get; init; }
        public required IReadOnlyList<SeatInput> Seats { get; init; }

        protected override Task ExecuteAsync(ICommandExecutionContext executionContext)
        {
            return Task.CompletedTask;
        }
    }

    private class AssignSeatsCommand : Command
    {
        public required IReadOnlyDictionary<string, SeatInput?> SeatsByCode { get; init; }
        public required Dictionary<int, SeatInput> SeatsById { get; init; }

        protected override Task ExecuteAsync(ICommandExecutionContext executionContext)
        {
            return Task.CompletedTask;
        }
    }

    private class ArrangeSeatsCommand : Command
    {
        public required IReadOnlyList<IReadOnlyList<SeatInput>> SeatRows { get; init; }
        public required Dictionary<string, List<SeatInput>> SeatsByZone { get; init; }

        protected override Task ExecuteAsync(ICommandExecutionContext executionContext)
        {
            return Task.CompletedTask;
        }
    }

    private class UpdateBookingCommand : Command
    {
        public required Updatable<PassengerInput> Passenger { get; init; }
        public required Updatable<IReadOnlyList<SeatInput>> Seats { get; init; }

        protected override Task ExecuteAsync(ICommandExecutionContext executionContext)
        {
            return Task.CompletedTask;
        }
    }

    private class OwnRulesOnlyCommand : Command
    {
        public required PassengerInput Passenger { get; init; }

        public override Task<ValidationErrors> ValidateAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new ValidationErrors());
        }

        protected override Task ExecuteAsync(ICommandExecutionContext executionContext)
        {
            return Task.CompletedTask;
        }
    }

    private class ContactCommand : Command
    {
        public required ContactInput Contact { get; init; }

        protected override Task ExecuteAsync(ICommandExecutionContext executionContext)
        {
            return Task.CompletedTask;
        }
    }

    private class DateRangeCommand : Command
    {
        public required DateOnly Start { get; init; }
        public required DateOnly End { get; init; }

        public override async Task<ValidationErrors> ValidateAsync(CancellationToken cancellationToken)
        {
            var errors = await ValidateInputsAsync(cancellationToken);
            if (End < Start)
            {
                errors.AddForObject("The end cannot be before the start.");
            }

            return errors;
        }

        protected override Task ExecuteAsync(ICommandExecutionContext executionContext)
        {
            return Task.CompletedTask;
        }
    }

    private class RegisterPassengerCommand : Command<int>
    {
        public required PassengerInput Passenger { get; init; }

        protected override Task<int> ExecuteAsync(ICommandExecutionContext executionContext)
        {
            return Task.FromResult(1);
        }
    }

    private class NullKeySeats : IEnumerable<KeyValuePair<string?, SeatInput>>
    {
        public IEnumerator<KeyValuePair<string?, SeatInput>> GetEnumerator()
        {
            yield return new KeyValuePair<string?, SeatInput>(null, new SeatInput { Row = 0 });
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    private class NullKeySeatsCommand : Command
    {
        public required NullKeySeats Seats { get; init; }

        protected override Task ExecuteAsync(ICommandExecutionContext executionContext)
        {
            return Task.CompletedTask;
        }
    }

    private class NullTextKey
    {
        public override string ToString()
        {
            return null!;
        }
    }

    private class NullValidationInput : Input
    {
        public override Task<ValidationErrors> ValidateAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<ValidationErrors>(null!);
        }
    }

    private class NullInputValidationCommand : Command
    {
        public required NullValidationInput Input { get; init; }

        protected override Task ExecuteAsync(ICommandExecutionContext executionContext)
        {
            return Task.CompletedTask;
        }
    }

    private class NullValidationCommand : Command
    {
        public override Task<ValidationErrors> ValidateAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<ValidationErrors>(null!);
        }

        protected override Task ExecuteAsync(ICommandExecutionContext executionContext)
        {
            return Task.CompletedTask;
        }
    }
}
