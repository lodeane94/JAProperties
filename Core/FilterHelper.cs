using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace SS.Code
{
    public static class FilterHelper
    {
        private static string newFilterString = string.Empty;
        private static string conditionToBeRemoved = string.Empty;
        //used to create string with the LIKE keyword to be used in the where clause of the sql statement
        public static string filterOnHelperLike(string filterString, string columnName,string columnValue)
        {
            if (String.IsNullOrEmpty(filterString))
	        {
                newFilterString = columnName + " %LIKE% " + columnValue;
                return newFilterString;
	        }

            newFilterString = filterString + " AND " + columnName + " %LIKE% " + columnValue;

            return newFilterString;
        }
        //used to create string with the BETWEEN keyword to be used in the where clause of the sql statement
        public static string filterOnHelperBetween(string filterString, string columnName, string columnMinValue, string columnMaxValue)
        {
            if (String.IsNullOrEmpty(filterString))
            {
                newFilterString = columnName + " BETWEEN " + columnMinValue + "AND" + columnMaxValue;
                return newFilterString;
            }

            newFilterString = filterString + " AND " + columnName + " BETWEEN " + columnMinValue + "AND" + columnMaxValue;

            return newFilterString;
        }
        //used to remove the particular condition from the where clause for the LIKE keyword
        public static string filterOffHelperLike(string filterString, string columnName, string columnValue)
        {
            Regex regex = new Regex(@"" + conditionToBeRemoved, RegexOptions.IgnoreCase);

            if (!filterString.Contains("AND"))
            {
                conditionToBeRemoved = columnName + " %LIKE% " + columnValue;

                newFilterString = regex.Replace(filterString,"");

                return newFilterString;
            }

            conditionToBeRemoved = "AND " + columnName + " %LIKE% " + columnValue;

            newFilterString = regex.Replace(filterString, "");

            return newFilterString;
        }

        //used to remove the particular condition from the where clause for the LIKE keyword
        public static string filterOffHelperBetween(string filterString, string columnName, string columnMinValue, string columnMaxValue)
        {
            Regex conRemovedRegex = new Regex(@"" + conditionToBeRemoved, RegexOptions.IgnoreCase);

            Regex multiRegexCheck = new Regex(@"" + columnMaxValue + @"\sAND", RegexOptions.IgnoreCase);

            MatchCollection mathes = multiRegexCheck.Matches(filterString);

            if (mathes.Count < 1)
            {
                conditionToBeRemoved = columnName + " BETWEEN " + columnMinValue + " AND " + columnMaxValue;

                newFilterString = conRemovedRegex.Replace(filterString, "");

                return newFilterString;
            }

            conditionToBeRemoved = "AND " + columnName + " BETWEEN " + columnMinValue + " AND " + columnMaxValue;

            newFilterString = conRemovedRegex.Replace(filterString, "");

            return newFilterString;
        }
    }
}