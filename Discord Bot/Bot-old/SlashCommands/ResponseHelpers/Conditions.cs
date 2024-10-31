using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.SlashCommands.ResponseHelpers
{
    public class Conditions
    {
        bool makeSureEmbedTitleMatches;
        string embedTitle;

        bool makeSureFieldDoesNotExist;
        string fieldNameThatShouldNotExist;

        bool makeSureFieldExist;
        string fieldNameThatShouldExist;

        bool makeSureFieldsHaveValues;
        Dictionary<string, string> fieldValuesToCheck = new();

        bool makeSureIncomingValueMatches;
        string incomingValueToCompareAgainst;

        bool makeSureIncomingValueDoesNotMatch;
        string incomingValueToCompareAgainstToNotMatch;

        bool makeSureIncomingValueMatchesFollowing;
        string[] compareIncomingValueAgainstAll;

        bool makeSureModalTitleMatches;
        string modalTitleToCompareAgainst;

        bool hasCustomConditions;
        List<Func<Request, bool>> customConditions = new();

        public Conditions MakeSureEmbedTitleMatches(string title)
        {
            makeSureEmbedTitleMatches = true;
            embedTitle = title;
            return this;
        }

        public Conditions MakeSureFieldDoesNotExist(string fieldName)
        {
            makeSureFieldDoesNotExist = true;
            fieldNameThatShouldNotExist = fieldName;
            return this;
        }

        public Conditions MakeSureFieldExist(string fieldName)
        {
            makeSureFieldExist = true;
            fieldNameThatShouldExist = fieldName;
            return this;
        }

        public Conditions MakeSureFieldHasValue(string fieldName, string fieldValue)
        {
            makeSureFieldsHaveValues = true;
            fieldValuesToCheck[fieldName] = fieldValue;
            return this;
        }

        public Conditions MakeSureIncomingValueMatches(string value)
        {
            makeSureIncomingValueMatches = true;
            incomingValueToCompareAgainst = value;
            return this;
        }
        public Conditions MakeSureIncomingValueDoesNotMatch(string value)
        {
            makeSureIncomingValueDoesNotMatch = true;
            incomingValueToCompareAgainstToNotMatch = value;
            return this;
        }
        public Conditions MakeSureIncomingValueMatchesFollowing(params string[] values)
        {
            makeSureIncomingValueMatchesFollowing = true;
            compareIncomingValueAgainstAll = values;
            return this;
        }
        public Conditions MakeSureModalTitleMatches(string modalTitle)
        {
            makeSureModalTitleMatches = true;
            modalTitleToCompareAgainst = modalTitle;
            return this;
        }

        public Conditions AddCustomCondition(Func<Request, bool> condition)
        {
            hasCustomConditions = true;
            customConditions.Add(condition);
            return this;
        }

        public bool CheckConditions(Request request)
        {
            if(makeSureEmbedTitleMatches)
            {
                //The second comparison is being done because there are existing verify messages that have embed title as "Verification Required".
                //The new ones will be Account (Verification)
                if(request.Embed.Title != embedTitle && request.Embed.Title != HelperStrings.verificationRequired)
                {
                    return false;
                }
            }

            if(makeSureFieldDoesNotExist && request.HasEmbedField(fieldNameThatShouldNotExist, out _))
            {
                return false;
            }

            if (makeSureFieldExist && !request.HasEmbedField(fieldNameThatShouldExist, out _))
            {
                return false;
            }

            if (makeSureFieldsHaveValues)
            {
                foreach(var key in fieldValuesToCheck.Keys)
                {                    
                    if(!request.HasEmbedField(key, out var value))
                    {
                        return false;
                    }

                    if(fieldValuesToCheck[key] != value)
                    {
                        return false;
                    }
                }
            }

            if(makeSureIncomingValueMatches && request.IncomingValue != incomingValueToCompareAgainst)
            {
                return false;
            }
            if (makeSureIncomingValueDoesNotMatch && request.IncomingValue == incomingValueToCompareAgainstToNotMatch)
            {
                return false;
            }
            if (makeSureIncomingValueMatchesFollowing && !compareIncomingValueAgainstAll.Contains(request.IncomingValue))
            {
                return false;
            }
            if (makeSureModalTitleMatches && request.IncomingModalName != modalTitleToCompareAgainst)
            {
                return false;
            }

            if(hasCustomConditions)
            {
                foreach(var condition in customConditions)
                {
                    if(!condition.Invoke(request))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
