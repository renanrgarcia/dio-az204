using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;

namespace function_cpfValidator
{
    public static class FnValidateCpf
    {
        [FunctionName("fnvalidatecpf")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Starting CPF validation function.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            string cpf = req.Query["cpf"];
            if (string.IsNullOrWhiteSpace(cpf))
            {
                cpf = data?.cpf?.ToString();
            }

            if (string.IsNullOrWhiteSpace(cpf))
            {
                return new BadRequestObjectResult("Please, inform the CPF.");
            }

            bool isValid = CpfValidator.IsValid(cpf);

            if (isValid)
            {
                var responseMessage = new
                {
                    cpf,
                    message = "Valid CPF."
                };
                return new OkObjectResult(responseMessage);
            }
            else
            {
                var responseMessage = new
                {
                    cpf,
                    message = "Invalid CPF."
                };
                return new OkObjectResult(responseMessage);
            }
        }
    }

    public static class CpfValidator
    {
        public static bool IsValid(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            // Remove non-digit characters
            var digits = new string(cpf.Where(char.IsDigit).ToArray());

            if (digits.Length != 11)
                return false;

            // Reject CPFs with all identical digits (e.g., 00000000000)
            if (digits.Distinct().Count() == 1)
                return false;

            // Calculate first check digit
            if (!ValidateDigit(digits, 9))
                return false;

            // Calculate second check digit
            if (!ValidateDigit(digits, 10))
                return false;

            return true;
        }

        private static bool ValidateDigit(string digits, int position)
        {
            // position: 9 for first check digit (index 9), 10 for second (index 10)
            int sum = 0;
            int weight = position + 1; // for position 9 -> weight starts at 10, for 10 -> 11

            for (int i = 0; i < position; i++)
            {
                sum += (digits[i] - '0') * (weight - i);
            }

            int remainder = sum % 11;
            int calculated = remainder < 2 ? 0 : 11 - remainder;

            return calculated == (digits[position] - '0');
        }
    }
}
