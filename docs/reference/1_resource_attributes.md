# Resource Attributes

Resource attributes declare how commands or queries relate to resource-oriented operations.

They are one example of declarative intent.

They do not execute behavior and they do not define infrastructure concerns. They expose metadata about what a command or query does.

## Attributes

| Attribute | Intent |
| --- | --- |
| `RetrieveResourceAttribute` | Retrieve one resource. |
| `ListResourcesAttribute` | List resources. |
| `CreateResourceAttribute` | Create a resource. |
| `UpdateResourceAttribute` | Partially update a resource. |
| `ReplaceResourceAttribute` | Replace a resource. |
| `DeleteResourceAttribute` | Delete a resource. |
| `ActionOnResourceAttribute` | Execute a named action on a resource. |

## Parameters

Resource attributes generally include:

- `resource`: the resource name or resource hierarchy.
- `key`: the key or keys that identify a resource level, when needed.

`ActionOnResourceAttribute` also includes:

- `action`: the action name.

## Resource Hierarchy

The `resource` value can declare multiple levels using `/`.

```csharp
[ListResources("users/active users/related users")]
```

Each segment represents one resource level:

- `users`
- `active users`
- `related users`

The exact normalization of names, such as casing, pluralization, or separators, belongs to the code that consumes this metadata.

## Keys

The `key` value declares which parameters identify each resource level.

```csharp
[RetrieveResource("user", key: "code")]
```

This declares that `code` identifies the `user` resource.

Keys are matched to resource levels by their position. Resource levels are separated with `/`, and key levels are separated with `/` in the same way.

## Subresources And Key Placement

A key applies to the resource or subresource at the same hierarchical level.

```text
resource: Resource/Subresource
key:      key1/key2
```

This means:

- `key1` identifies `Resource`.
- `key2` identifies `Subresource`.

If a level has no key, leave that key level empty.

```text
resource: Resource/Subresource
key:      /key
```

This means:

- `Resource` has no key.
- `Subresource` is identified by `key`.

## Multiple Keys Per Level

A resource level can have more than one key. Separate keys for the same level with `,`.

```text
resource: Resource/Subresource
key:      key1,key2/key3
```

This means:

- `Resource` is identified by `key1` and `key2`.
- `Subresource` is identified by `key3`.

## Key Mapping Examples

| Resource declaration | Key declaration | Meaning |
| --- | --- | --- |
| `Resource/Subresource` | `key1/key2` | `Resource` has `key1`; `Subresource` has `key2`. |
| `Resource/Subresource` | `key` | `Resource` has `key`; `Subresource` has no key. |
| `Resource/Subresource` | `/key` | `Resource` has no key; `Subresource` has `key`. |
| `Resource/Subresource1/Subresource2` | `key1//key2` | `Resource` has `key1`; `Subresource1` has no key; `Subresource2` has `key2`. |
| `Resource/Subresource1/Subresource2` | `//key` | Only `Subresource2` has `key`. |
| `Resource/Subresource` | `key1,key2` | `Resource` has `key1` and `key2`; `Subresource` has no key. |
| `Resource/Subresource` | `key1/key2,key3` | `Resource` has `key1`; `Subresource` has `key2` and `key3`. |

## Canonical Resource Address Shape

The resource hierarchy and keys provide enough information to describe a canonical resource address shape:

```text
resource
    : resource_name
      (resource_key)*
      (
          '/' subresource_name
          (subresource_key)*
      )*
      ('/' action)?
    ;

resource_key
    : '/' key_name '-' key_value
    ;
```

For example:

```text
resource: Resource/Subresource
key:      key1/key2
shape:    resource/key1-{key1}/subresource/key2-{key2}
```

This shape is derived from the resource metadata. Transport-specific or infrastructure-specific representations are outside the scope of the attribute itself.

## Example

```csharp
[RetrieveResource("user", key: "code")]
public class RetrieveUserByCodeCommand(int code) : Command<string>
{
    // ...
}
```

This declares a read use case that retrieves a `user` identified by `code`.

## Action Example

```csharp
[ActionOnResource("user", action: "rename", key: "name")]
public class RenameUserCommand(string name, string newName) : Command<string>
{
    // ...
}
```

This declares an action named `rename` on a `user` identified by `name`.
