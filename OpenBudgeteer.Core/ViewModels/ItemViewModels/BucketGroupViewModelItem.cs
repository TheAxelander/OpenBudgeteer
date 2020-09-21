using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Common.EventClasses;

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

        private bool _isCollapsed;
        public bool IsCollapsed
        {
            get => _isCollapsed;
            set => Set(ref _isCollapsed, value);
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
        
        public event EventHandler<ViewModelReloadEventArgs> ViewModelReloadRequired;

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

        public Tuple<bool,string> SaveModification()
        {
            try
            {
                using (var dbContext = new DatabaseContext(_dbOptions))
                {
                    dbContext.UpdateBucketGroup(BucketGroup);
                }
                ViewModelReloadRequired?.Invoke(this, new ViewModelReloadEventArgs(this));
                InModification = false;
                _oldBucketGroup = null;
            }
            catch (Exception e)
            {
                return new Tuple<bool, string>(false, $"Unable to write changes to database: {e.Message}");
            }
            return new Tuple<bool, string>(true, string.Empty);
        }

        public Tuple<bool,string> DeleteGroup()
        {
            try
            {
                if (Buckets.Count > 0) throw new Exception("Groups with Buckets cannot be deleted.");

                using (var dbContext = new DatabaseContext(_dbOptions))
                {
                    dbContext.DeleteBucketGroup(BucketGroup);
                }
            }
            catch (Exception e)
            {
                return new Tuple<bool, string>(false, $"Unable to delete Bucket Group: {e.Message}");
            }
            return new Tuple<bool, string>(true, string.Empty);
        }

        public Tuple<bool,string> MoveGroup(int positions)
        {
            if (positions == 0) return new Tuple<bool, string>(true, string.Empty);
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var bucketGroupCount = dbContext.BucketGroup.Count();
                        var targetPosition = BucketGroup.Position + positions;
                        if (targetPosition < 1) targetPosition = 1;
                        if (targetPosition > bucketGroupCount) targetPosition = bucketGroupCount;
                        if (targetPosition == BucketGroup.Position) return new Tuple<bool, string>(true, string.Empty); // Group is already at the end or top. No further action
                        // Move Group in an interim List
                        var existingBucketGroups = new ObservableCollection<BucketGroup>();
                        foreach (var bucketGroup in dbContext.BucketGroup.OrderBy(i => i.Position))
                        {
                            existingBucketGroups.Add(bucketGroup);
                        }
                        existingBucketGroups.Move(BucketGroup.Position-1, targetPosition-1);
                        
                        // Update Position number
                        var newPosition = 1;
                        foreach (var bucketGroup in existingBucketGroups)
                        {
                            bucketGroup.Position = newPosition;
                            dbContext.UpdateBucketGroup(bucketGroup);
                            newPosition++;
                        }

                        transaction.Commit();
                        ViewModelReloadRequired?.Invoke(this, new ViewModelReloadEventArgs(this));
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        return new Tuple<bool, string>(false, $"Unable to move Bucket Group: {e.Message}");
                    }
                }
            }
            
            return new Tuple<bool, string>(true, string.Empty);
        }

        public BucketViewModelItem CreateBucket()
        {
            var newBucket = new BucketViewModelItem(_dbOptions, BucketGroup, CurrentMonth);
            // Hand over ViewModel changes
            newBucket.ViewModelReloadRequired += (sender, args) =>
                ViewModelReloadRequired?.Invoke(this, new ViewModelReloadEventArgs(this));
            Buckets.Add(newBucket);
            return newBucket;
        }
    }
}
