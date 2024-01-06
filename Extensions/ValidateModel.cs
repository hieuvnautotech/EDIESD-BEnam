namespace ESD_EDI_BE.Extensions
{
    public static class ValidateModel
    {
        public static bool CheckValid<T, U>(T source, U destination)
        {
            if (source == null)
            {
                return false; //Object Invalid
            }

            var model = AutoMapperConfig<T, U>.Map(source);
            var isValid = SimpleValidator.IsModelValid(model);
            if (!isValid)
            {
                return false; //Object Invalid
            }
            return true;
        }
    }
}
