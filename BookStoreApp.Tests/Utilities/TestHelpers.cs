using System.ComponentModel.DataAnnotations;

namespace BookStoreApp.Tests.Utilities
{
    public static class TestHelpers
    {
        public static List<ValidationResult> ValidateModel<T>(T model) where T : class
        {
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, true);
            return results;
        }

        public static bool IsModelValid<T>(T model) where T : class
        {
            var validationResults = ValidateModel(model);
            return !validationResults.Any();
        }

        public static List<string> GetValidationErrorsForProperty<T>(T model, string propertyName) where T : class
        {
            var validationResults = ValidateModel(model);
            return validationResults
                .Where(vr => vr.MemberNames.Contains(propertyName))
                .Select(vr => vr.ErrorMessage ?? "Unknown error")
                .ToList();
        }

        public static void AssertHasValidationError<T>(T model, string propertyName, string expectedError = null) where T : class
        {
            var errors = GetValidationErrorsForProperty(model, propertyName);
            
            if (!errors.Any())
            {
                throw new InvalidOperationException($"Expected validation error for property '{propertyName}' but none was found.");
            }

            if (!string.IsNullOrEmpty(expectedError) && !errors.Any(e => e.Contains(expectedError)))
            {
                throw new InvalidOperationException($"Expected validation error containing '{expectedError}' for property '{propertyName}' but found: {string.Join(", ", errors)}");
            }
        }

        public static void AssertNoValidationError<T>(T model, string propertyName) where T : class
        {
            var errors = GetValidationErrorsForProperty(model, propertyName);
            
            if (errors.Any())
            {
                throw new InvalidOperationException($"Unexpected validation error for property '{propertyName}': {string.Join(", ", errors)}");
            }
        }

        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GenerateRandomEmail()
        {
            var username = GenerateRandomString(8);
            var domain = GenerateRandomString(5);
            return $"{username}@{domain}.com";
        }

        public static decimal GenerateRandomDecimal(decimal min = 0m, decimal max = 100m)
        {
            var random = new Random();
            var range = max - min;
            var randomValue = (decimal)random.NextDouble() * range;
            return Math.Round(min + randomValue, 2);
        }

        public static int GenerateRandomInt(int min = 1, int max = 100)
        {
            var random = new Random();
            return random.Next(min, max + 1);
        }

        public static DateTime GenerateRandomPastDate(int maxDaysAgo = 365)
        {
            var random = new Random();
            var daysAgo = random.Next(1, maxDaysAgo + 1);
            return DateTime.UtcNow.AddDays(-daysAgo);
        }

        public static bool AreDecimalsEqual(decimal value1, decimal value2, decimal precision = 0.01m)
        {
            return Math.Abs(value1 - value2) < precision;
        }

        public static async Task<bool> WaitForConditionAsync(Func<bool> condition, TimeSpan timeout)
        {
            var endTime = DateTime.UtcNow + timeout;
            
            while (DateTime.UtcNow < endTime)
            {
                if (condition())
                {
                    return true;
                }
                
                await Task.Delay(100); // Check every 100ms
            }
            
            return false;
        }

        public static List<T> CreateTestDataSet<T>(Func<int, T> factory, int count)
        {
            var result = new List<T>();
            for (int i = 0; i < count; i++)
            {
                result.Add(factory(i));
            }
            return result;
        }
    }
}
