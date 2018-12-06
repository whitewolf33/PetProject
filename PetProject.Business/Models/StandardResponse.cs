namespace PetProject.BusinessLayer.Models
{
    public class StandardResponse<T> where T : class
    {
        private const string _genericError = "Oops! Something went wrong. Please try again.";

        public bool Success { get; set; }

        public string ErrorMessage { get; set; }

        public T Result { get; set; }

        public static StandardResponse<T> GenericError(string errorMsg = "")
        {
            var genericError = new StandardResponse<T>();
            genericError.Success = false;
#if RELEASE
            genericError.ErrorMessage = _genericError;
#else
            genericError.ErrorMessage = errorMsg ?? _genericError;
#endif
            genericError.Result = default(T);
            return genericError;

        }
    }
}