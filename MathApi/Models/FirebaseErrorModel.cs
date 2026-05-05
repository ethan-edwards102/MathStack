namespace MathApis.Models;

public class FirebaseErrorModel
{
    public FirebaseError? Error { get; set; }
}

public class FirebaseError
{
    public int Code { get; set; }
    public string? Message { get; set; }
    public List<Error>? Errors { get; set; }
}