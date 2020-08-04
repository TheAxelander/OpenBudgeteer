using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace OpenBudgeteer.Core.ViewModels.ItemViewModels
{
    public class BucketGroupViewModelItem : ViewModelBase
    {
        private BucketGroup _bucketGroup;
        public BucketGroup BucketGroup
        {
            get => _bucketGroup;
            internal set => Set(ref _bucketGroup, value);
        }

        private decimal _totalBalance;
        public decimal TotalBalance
        {
            get => _totalBalance;
            set => Set(ref _totalBalance, value);
        }

        private bool _isHovered;
        public bool IsHovered
        {
            get => _isHovered;
            set => Set(ref _isHovered, value);
        }

        private ObservableCollection<BucketViewModelItem> _buckets;
        public ObservableCollection<BucketViewModelItem> Buckets
        {
            get => _buckets;
            set => Set(ref _buckets, value);
        }

        private bool _inModification;
        public bool InModification
        {
            get => _inModification;
            set => Set(ref _inModification, value);
        }
        
        internal event ViewModelReloadRequiredHandler ViewModelReloadRequired;
        internal delegate void ViewModelReloadRequiredHandler(BucketGroupViewModelItem sender);
        internal DateTime CurrentMonth;
        private readonly DbContextOptions<DatabaseContext> _dbOptions;
        private BucketGroup _oldBucketGroup;

        public BucketGroupViewModelItem(DbContextOptions<DatabaseContext> dbOptions)
        {
            Buckets = new ObservableCollection<BucketViewModelItem>();
            InModification = false;
            _dbOptions = dbOptions;
        }

        public BucketGroupViewModelItem(DbContextOptions<DatabaseContext> dbOptions, BucketGroup bucketGroup, DateTime currentMonth) : this(dbOptions)
        {
            BucketGroup = bucketGroup;
            CurrentMonth = currentMonth;
        }

        public void StartModification()
        {
            _oldBucketGroup = new BucketGroup()
            {
                BucketGroupId = BucketGroup.BucketGroupId,
                Name = BucketGroup.Name,
                Position = BucketGroup.Position
            };
            InModification = true;
        }

        public void CancelModification()
        {
            BucketGroup = _oldBucketGroup;
            InModification = false;
            _oldBucketGroup = null;
        }

        public void SaveModification()
        {
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                dbContext.UpdateBucketGroup(BucketGroup);
            }
            ViewModelReloadRequired?.Invoke(this);
            InModification = false;
            _oldBucketGroup = null;
        }

        public void DeleteGroup()
        {
            if (Buckets.Count > 0)
            {
                //TODO: Display Message that there are still Buckets in that group
            }
            else
            {
                using (var dbContext = new DatabaseContext(_dbOptions))
                {
                    dbContext.DeleteBucketGroup(BucketGroup);
                }
                ViewModelReloadRequired?.Invoke(this);
            }
        }

        public BucketViewModelItem CreateBucket()
        {
            var newBucket = new BucketViewModelItem(_dbOptions, BucketGroup, CurrentMonth);
            newBucket.ViewModelReloadRequired += (sender) =>
            {
                // Hand over ViewModel changes
                ViewModelReloadRequired?.Invoke(this);
            };
            Buckets.Add(newBucket);
            return newBucket;
        }
    }
}
