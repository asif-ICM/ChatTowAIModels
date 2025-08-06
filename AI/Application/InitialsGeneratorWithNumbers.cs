using System.Collections.Generic;
using System.Linq;
using Util;
namespace AI.Application
{
    internal class InitialsGeneratorBase
    {
        protected readonly List<string> _existingInitials;
        protected readonly List<string> _offensiveWordList;
        internal InitialsGeneratorBase(List<string> existingInitials, List<string> offensiveWordList)
        {
            _offensiveWordList = offensiveWordList;
            _existingInitials = new List<string>();
            if (existingInitials.IsValidList())
            {
                _existingInitials = existingInitials;
            }
        }
        protected bool IsValidInitial(string potentialInitials)
        {
            if (IsInitialUnique(potentialInitials) && IsNonOffensiveWord(potentialInitials))
            {
                return true;
            }
            return false;
        }
        protected bool IsInitialUnique(string potentialInitial)
        {
            if (NoExistingInitials())
                return true;
            return !_existingInitials.Contains(potentialInitial);
        }

        private bool NoExistingInitials()
        {
            if (!_existingInitials.IsValidList())
            {
                return true;
            }
            return false;
        }
        protected bool IsOffensiveWordListEmpty()
        {
            if (!_offensiveWordList.IsValidList())
            {
                return true;
            }
            return false;
        }
        protected bool IsNonOffensiveWord(string potentialInitial)
        {
            return !_offensiveWordList.Contains(potentialInitial);
        }
        protected string RemoveWhiteSpaces(string input)
        {
            if (input.IsNullOrEmpty())
            {
                return input;
            }
            return input.Replace(" ", "");
        }
        protected string SelectUnusedInitial(List<string> possibleInitials)
        {
            return possibleInitials.FirstOrDefault(initial => !_existingInitials.Contains(initial));
        }

        protected void FilterOutOffensiveWords(List<string> possibleInitials)
        {
            possibleInitials.RemoveAll(initial => _offensiveWordList.Contains(initial));
        }
        protected string RemoveNonAlphabeticCharacters(string input)
        {
            input = RemoveWhiteSpaces(input);
            input = input.RemoveNonAlphabeticCharacters();
            return input;
        }
    }

    internal class InitialsGeneratorWithNumbers : InitialsGeneratorBase
    {

        internal InitialsGeneratorWithNumbers(List<string> existingInitials, List<string> offensiveWordList)
            : base(existingInitials, offensiveWordList)
        {

        }
        internal string GenerateInitials(string firstName, string middleName, string lastName)
        {
            string initial = "";
            if (IsOffensiveWordListEmpty())
            {
                return initial;
            }
            firstName = RemoveNonAlphabeticCharacters(firstName);
            middleName = RemoveNonAlphabeticCharacters(middleName);
            lastName = RemoveNonAlphabeticCharacters(lastName);
            if (firstName.IsNullOrEmpty())
            {
                return initial;
            }
            if ( lastName.IsNullOrEmpty())
            {
                return initial;
            }
            initial = ($"{firstName[0]}{lastName[0]}").ToUpper();
            if (IsValidInitial(initial))
                return initial;
            if (middleName.IsNotNullOrEmpty())
                initial = ($"{firstName[0]}{middleName[0]}{lastName[0]}").ToUpper();
            if (IsValidInitial(initial))
                return initial;
            initial = ($"{firstName[0]}{lastName[0]}").ToUpper();
            if (!IsNonOffensiveWord(initial))
            {
                return "";
            }
            initial = GenerateUniqueInitials(initial);
            if (initial.IsNotNullOrEmpty())
            {
                return initial;
            }
            if (middleName.IsNotNullOrEmpty())
            {
                initial = ($"{firstName[0]}{middleName[0]}{lastName[0]}").ToUpper();
                if (!IsNonOffensiveWord(initial))
                {
                    return "";
                }
                initial = GenerateUniqueInitials(initial);
            }
            return initial;
        }

        private string GenerateUniqueInitials(string initial)
        {
            initial = initial.ToUpper();
            string potentialInitials = initial;
            int loopLength = 100;
            if (potentialInitials.Length == 3)
            {
                loopLength = 10;
            }
            for (int i = 1; i < loopLength; i++)
            {
                if (IsValidInitial(potentialInitials))
                {
                    return potentialInitials;
                }
                potentialInitials = $"{initial}{i}";
            }
            return "";
        }
    }
}