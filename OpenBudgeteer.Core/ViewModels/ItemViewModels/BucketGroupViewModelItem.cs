using OpenBudgeteer.Core.Common.Database;
using OpenBudgeteer.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Common.EventClasses;

namespace OpenBudgeteer.Core.ViewModels.ItemViewModels
{
    public class BucketGroupViewModelItem : ViewModelBase
    {
        private BucketGroup _bucketGroup;
        /// <summary>
        /// Reference to model object in the database
        /// </summary>
        public BucketGroup BucketGroup
        {
            get => _bucketGroup;
            internal set => Set(ref _bucketGroup, value);
        }

        private decimal _totalBalance;
        /// <summary>
        /// Balance of all Buckets assigned to the BucketGroup
        /// </summary>
        public decimal TotalBalance
        {
            get => _totalBalance;
            set => Set(ref _totalBalance, value);
        }

        private bool _isHovered;
        /// <summary>
        /// Helper property to check if the cursor hovers over the entry in the UI
        /// </summary>
        public bool IsHovered
        {
            get => _isHovered;
            set => Set(ref _isHovered, value);
        }

        private bool _isCollapsed;
        /// <summary>
        /// Helper property to check if the list of assigned Buckets is collapsed
        /// </summary>
        public bool IsCollapsed
        {
            get => _isCollapsed;
            set => Set(ref _isCollapsed, value);
        }

        private ObservableCollection<BucketViewModelItem> _buckets;
        /// <summary>
        /// Collection of Buckets assigned to this BucketGroup
        /// </summary>
        public ObservableCollection<BucketViewModelItem> Buckets
        {
            get => _buckets;
            set => Set(ref _buckets, value);
        }

        private bool _inModification;
        /// <summary>
        /// Helper property to check if the BucketGroup is currently modified
        /// </summary>
        public bool InModification
        {
            get => _inModification;
            set => Set(ref _inModification, value);
        }
        
        private readonly DateTime _currentMonth;
        private readonly DbContextOptions<DatabaseContext> _dbOptions;
        private BucketGroup _oldBucketGroup;

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="dbOptions">Options to connect to a database</param>
        public BucketGroupViewModelItem(DbContextOptions<DatabaseContext> dbOptions)
        {
            Buckets = new ObservableCollection<BucketViewModelItem>();
            InModification = false;
            _dbOptions = dbOptions;
        }

        /// <summary>
        /// Initialize ViewModel based on an existing <see cref="BucketGroup"/> object and a specific YearMonth
        /// </summary>
        /// <param name="dbOptions">Options to connect to a database</param>
        /// <param name="bucketGroup">BucketGroup instance</param>
        /// <param name="currentMonth">YearMonth that should be used</param>
        public BucketGroupViewModelItem(DbContextOptions<DatabaseContext> dbOptions, BucketGroup bucketGroup, DateTime currentMonth) : this(dbOptions)
        {
            BucketGroup = bucketGroup;
            _currentMonth = currentMonth;
        }

        /// <summary>
        /// Helper method to start modification process and creating a backup of current values
        /// </summary>
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

        /// <summary>
        /// Stops modification and restores previous values
        /// </summary>
        public void CancelModification()
        {
            BucketGroup = _oldBucketGroup;
            InModification = false;
            _oldBucketGroup = null;
        }

        /// <summary>
        /// Updates a record in the database based on <see cref="BucketGroup"/> object
        /// </summary>
        /// <remarks>Triggers <see cref="ViewModelReloadRequired"/></remarks>
        /// <returns>Object which contains information and results of this method</returns>
        public ViewModelOperationResult SaveModification()
        {
            try
            {
                using (var dbContext = new DatabaseContext(_dbOptions))
                {
                    dbContext.UpdateBucketGroup(BucketGroup);
                }
                InModification = false;
                _oldBucketGroup = null;
                return new ViewModelOperationResult(true, true);
            }
            catch (Exception e)
            {
                return new ViewModelOperationResult(false, $"Unable to write changes to database: {e.Message}");
            }
        }

        /// <summary>
        /// Moves the position of the BucketGroup according to the passed value. Updates positions for all other
        /// BucketGroups accordingly
        /// </summary>
        /// <param name="positions">Number of positions that BucketGroup needs to be moved</param>
        /// <remarks>Triggers <see cref="ViewModelReloadRequired"/></remarks>
        /// <returns>Object which contains information and results of this method</returns>
        public ViewModelOperationResult MoveGroup(int positions)
        {
            if (positions == 0) return new ViewModelOperationResult(true);
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
                        if (targetPosition == BucketGroup.Position) return new ViewModelOperationResult(true); // Group is already at the end or top. No further action
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
                        return new ViewModelOperationResult(true, true);
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        return new ViewModelOperationResult(false, $"Unable to move Bucket Group: {e.Message}");
                    }
                }
            }
        }

        public BucketViewModelItem CreateBucket()
        {
            var newBucket = new BucketViewModelItem(_dbOptions, BucketGroup, _currentMonth);
            Buckets.Add(newBucket);
            return newBucket;
        }
    }
}
