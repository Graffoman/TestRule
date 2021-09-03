using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UiPath.Studio.Activities.Api;
using UiPath.Studio.Activities.Api.Analyzer;
using UiPath.Studio.Activities.Api.Analyzer.Rules;
using UiPath.Studio.Analyzer.Models;

namespace TestRuleForCheckingAps
{
    public class RuleRepository : IRegisterAnalyzerConfiguration
    {
        public void Initialize(IAnalyzerConfigurationService workflowAnalyzerConfigService)
        {
            if (!workflowAnalyzerConfigService.HasFeature("WorkflowAnalyzerV4"))
                return;

            var forbiddenSelectorRule = new Rule<IActivityModel>("UnacceptableAS", "DE-LCK-001", InspectVariableForString);
            forbiddenSelectorRule.DefaultErrorLevel = System.Diagnostics.TraceLevel.Error;
            forbiddenSelectorRule.Parameters.Add("string_in_variable", new Parameter()
            {
                DefaultValue = "demo",
                Key = "string_in_variable",
                LocalizedDisplayName = "AS NamePart in Selector",
            });

            workflowAnalyzerConfigService.AddRule<IActivityModel>(forbiddenSelectorRule);
        }

        private InspectionResult InspectVariableForString(IActivityModel activityToInspect, Rule configuredRule)
        {
            var configuredString = configuredRule.Parameters["string_in_variable"]?.Value;

            if (String.IsNullOrEmpty(configuredString))
            {
                return new InspectionResult() { HasErrors = false };
            }

            if (activityToInspect.Properties.Count == 0)
            {
                return new InspectionResult() { HasErrors = false };
            }
            
            var messageList = new List<InspectionMessage>();

            foreach (var property in activityToInspect.Properties)
            {
                foreach (var argument in property.Arguments)
                {
                    if (argument.DisplayName == "Selector" && argument.DefinedExpression.Contains(configuredString))
                    {
                        messageList.Add(new InspectionMessage()
                        {
                            Message = $"В селекторе активности '{activityToInspect.DisplayName}' содержится '{configuredString}'"
                        });
                    }
                }

            }

            if (messageList.Count > 0)
            {
                return new InspectionResult()
                {
                    HasErrors = true,
                    InspectionMessages = messageList,
                    RecommendationMessage = "Не предусмотрено взаимодействие с данной АС",
                    ErrorLevel = configuredRule.ErrorLevel
                };
            }

            return new InspectionResult() { HasErrors = false };
        }
    }
}