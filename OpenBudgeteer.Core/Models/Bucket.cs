using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Drawing;

namespace OpenBudgeteer.Core.Models
{
    public class Bucket : BaseObject
    {
        private int _bucketId;
        public int BucketId
        {
            get => _bucketId;
            set => Set(ref _bucketId, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        private int _bucketGroupId;
        [Required]
        public int BucketGroupId
        {
            get => _bucketGroupId;
            set => Set(ref _bucketGroupId, value);
        }

        private string _colorCode;
        public string ColorCode
        {
            get => _colorCode;
            set => Set(ref _colorCode, value);
        }

        [NotMapped]
        public Color Color => string.IsNullOrEmpty(ColorCode) ? Color.LightGray : Color.FromName(ColorCode);

        private DateTime _validFrom;
        [Required]
        public DateTime ValidFrom
        {
            get => _validFrom;
            set => Set(ref _validFrom, value);
        }

        private bool _isInactive;
        public bool IsInactive
        {
            get => _isInactive;
            set => Set(ref _isInactive, value);
        }

        private DateTime _isInactiveFrom;
        public DateTime IsInactiveFrom
        {
            get => _isInactiveFrom;
            set => Set(ref _isInactiveFrom, value);
        }
    }
}
