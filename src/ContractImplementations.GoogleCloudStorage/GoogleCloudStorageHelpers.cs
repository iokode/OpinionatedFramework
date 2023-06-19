namespace IOKode.OpinionatedFramework.ContractImplementations.GoogleCloudStorage;

public static class GoogleCloudStorageHelpers
{
    public static GoogleCloudStorageClass GetStorageClassFromString(string str)
    {
        return str switch
        {
            "STANDARD" => GoogleCloudStorageClass.Standard,
            "COLDLINE" => GoogleCloudStorageClass.Coldline,
            "NEARLINE" => GoogleCloudStorageClass.Nearline,
            "ARCHIVE" => GoogleCloudStorageClass.Archive
        };
    }
}