using System;

namespace PetProject.BusinessLayer.Helpers
{
    public static class Extensions
    {
        #region String
        public static bool IsNullOrEmpty(this string stringVal)
        {
            if (String.IsNullOrEmpty(stringVal))
                return true;

            return stringVal.Trim().Length == 0;
        }
        #endregion        
    }
}
