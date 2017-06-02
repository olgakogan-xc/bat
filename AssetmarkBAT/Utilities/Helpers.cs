using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AssetmarkBAT.Utilities
{
    public class Helpers
    {
        public double ConvertToDouble(string input)
        {
            try
            {
                return Convert.ToDouble(input.Replace("$", "").Replace(",", "").Replace(" ", ""));
            }
            catch
            {
                return 0;
            }
        }

        public int RountDouble(double number)
        {
            if (number >= 0 && number <= 100)
            {
                return (int)Math.Round(number);
            }
            else if (number > 100 && number <= 10000)
            {
                return (int)Math.Round(number / 100) * 100;
            }
            else if (number > 10000)
            {
                return (int)Math.Round(number / 1000) * 1000;
            }
            else if (number > 100000)
            {
                return (int)Math.Round(number / 10000) * 10000;
            }
            else if (number > 1000000)
            {
                return (int)Math.Round(number / 100000) * 100000;
            }
            else
                return 0;
        }
    }

    public class BooleanRequiredAttribute : ValidationAttribute, IClientValidatable
    {
        public override bool IsValid(object value)
        {
            if (value is bool)
                return (bool)value;
            else
                return true;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(
            ModelMetadata metadata,
            ControllerContext context)
        {
            yield return new ModelClientValidationRule
            {
                ErrorMessage = FormatErrorMessage(metadata.GetDisplayName()),
                ValidationType = "booleanrequired"
            };
        }
    }
}