using System.Collections.Generic;
using System.Linq;
using Util;

namespace AI.Application
{
    internal class InitialsGenerator : InitialsGeneratorBase
    {
        internal InitialsGenerator(List<string> existingInitials, List<string> offensiveWordList)
            : base(existingInitials, offensiveWordList) { }
        internal string GenerateInitials(string firstName, string lastName)
        {
            string initial = "";
            if (IsOffensiveWordListEmpty())
            {
                return initial;
            }
            firstName = RemoveNonAlphabeticCharacters(firstName);
            lastName = RemoveNonAlphabeticCharacters(lastName);
            if (firstName.IsNullOrEmpty())
            {
                return initial;
            }
            if (lastName.IsNullOrEmpty())
            {
                return initial;
            }
            initial = GenerateUniqueInitials(firstName, lastName);
            if (initial.IsNotNullOrEmpty())
                return initial;
            string input = firstName + lastName;
            int initialsMaxLetters = 4;
            initial = GenerateUniqueInitialWithRandonLetters(input, initialsMaxLetters);
            if (initial.IsNotNullOrEmpty())
                return initial;
            initial = $"{firstName[0]}{lastName[0]}".ToUpper();
            return initial;
        }



        public string GenerateUniqueInitialWithRandonLetters(string input, int maxInitialsLength)
        {
            input = RemoveWhiteSpaces(input);
            for (int length = 2; length <= maxInitialsLength; length++)
            {
                List<string> possibleInitials = GetAllPermutations(input, length);
                FilterOutOffensiveWords(possibleInitials);
                string uniqueInitial = SelectUnusedInitial(possibleInitials);
                if (!string.IsNullOrEmpty(uniqueInitial))
                    return uniqueInitial;
            }
            return string.Empty;
        }
        private List<string> GetAllPermutations(string input, int permutationLength)
        {
            // permutations  is a math concepts , please read more for referance 
            List<string> permutations = new List<string>();
            bool[] used = new bool[input.Length];
            GeneratePermutations(input.ToUpper(), permutationLength, "", permutations, used);
            return permutations;
        }
        private void GeneratePermutations(string input, int permutationLength, string current, List<string> permutations, bool[] used)
        {
            if (current.Length == permutationLength)
            {
                permutations.Add(current.ToUpper()); // HashSet ensures uniqueness
                return;
            }

            for (int i = 0; i < input.Length; i++)
            {
                if (!used[i]) // Check if the character is already used
                {
                    used[i] = true; // Mark the character as used
                    GeneratePermutations(input, permutationLength, current + input[i], permutations, used);
                    used[i] = false; // Backtrack and mark the character as unused
                }
            }
        }
        private string GenerateUniqueInitials(string firstName, string lastName)
        {
            string potentialInitials = "";
            for (int i = 0; i < lastName.Length; i++)
            {
                potentialInitials = $"{firstName[0]}{lastName[i]}".ToUpper();
                if (IsValidInitial(potentialInitials))
                {
                    return potentialInitials;
                }
            }
            for (int i = 0; i < firstName.Length; i++)
            {
                potentialInitials = $"{firstName[i]}{lastName[0]}".ToUpper();
                if (IsValidInitial(potentialInitials))
                {
                    return potentialInitials;
                }
            }
            for (int i = 0; i < firstName.Length; i++)
            {
                for (int j = 0; j < lastName.Length; j++)
                {
                    potentialInitials = $"{firstName[i]}{lastName[j]}".ToUpper();
                    if (IsValidInitial(potentialInitials))
                    {
                        return potentialInitials;
                    }
                }
            }
            for (int i = 0; i < lastName.Length; i++)
            {
                for (int j = 0; j < firstName.Length; j++)
                {
                    potentialInitials = $"{lastName[i]}{firstName[j]}".ToUpper();
                    if (IsValidInitial(potentialInitials))
                    {
                        return potentialInitials;
                    }
                }
            }
            return "";
        }
    }
}
