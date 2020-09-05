using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Models;

namespace OpenBudgeteer.Core.ViewModels.ItemViewModels
{
    public class MappingRuleViewModelItem : ViewModelBase
    {
        private MappingRule _mappingRule;
        public MappingRule MappingRule
        {
            get => _mappingRule;
            set => Set(ref _mappingRule, value);
        }

        private string _ruleOutput;
        public string RuleOutput
        {
            get => _ruleOutput;
            set => Set(ref _ruleOutput, value);
        }
        
        private readonly DbContextOptions<DatabaseContext> _dbOptions;

        public MappingRuleViewModelItem(DbContextOptions<DatabaseContext> dbOptions)
        {
            _dbOptions = dbOptions;
        }

        public MappingRuleViewModelItem(DbContextOptions<DatabaseContext> dbOptions, MappingRule mappingRule) : this(dbOptions)
        {
            MappingRule = mappingRule;
            GenerateRuleOutput();
        }

        public void GenerateRuleOutput()
        {
            RuleOutput = MappingRule == null ? string.Empty :
                $"{MappingRule.ComparisonFieldOutput} " +
                $"{MappingRule.ComparisionTypeOutput} " +
                $"{MappingRule.ComparisionValue}";
        }
    }
}
