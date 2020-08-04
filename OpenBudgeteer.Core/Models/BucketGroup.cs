using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenBudgeteer.Core.Models
{
    public class BucketGroup : BaseObject
    {
        private int _bucketGroupId;
        public int BucketGroupId
        {
            get => _bucketGroupId;
            set => Set(ref _bucketGroupId, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        private int _position;
        [Required]
        public int Position
        {
            get => _position;
            set => Set(ref _position, value);
        }
    }
}
