using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text;

namespace OpenBudgeteer.Core.Models
{
    public class BucketRuleSet : BaseObject
    {
        private int _bucketRuleSetId;
        public int BucketRuleSetId
        {
            get => _bucketRuleSetId;
            set => Set(ref _bucketRuleSetId, value);
        }

        private int _priority;
        [Required]
        public int Priority
        {
            get => _priority;
            set => Set(ref _priority, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        private int _targetBucketId;
        [Required]
        public int TargetBucketId
        {
            get => _targetBucketId;
            set => Set(ref _targetBucketId, value);
        }
    }
}
