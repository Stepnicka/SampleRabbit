using System.Text.Json.Serialization;

namespace SampleRabbit.Shared.Dto
{
    /// <summary>
    ///     Basic API error response
    /// </summary>
    public record struct SimpleError
    {
        /// <summary>
        ///     Satus code for simple error object
        /// </summary>
        [JsonPropertyName(name: "code")]
        public required int Code { get; set; }

        /// <summary>
        ///     List of string error messages
        /// </summary>
        [JsonPropertyName(name: "errors")]
        public required string[] Errors { get; set; }

        public static SimpleError OnValidation(Shared.Error.ValidationError error) => new SimpleError { Code = 400, Errors = error.ValidationFailures.Select(v => v.ErrorMessage).ToArray() };
        public static SimpleError Generic => new SimpleError { Code = 500, Errors = ["V aplikaci došlo k neznámé chybě. Zkuste to později znovu."] };
    }
}
