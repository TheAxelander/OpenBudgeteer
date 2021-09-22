### 1.4 (20xx-xx-xx)

* [Add] Dialog for showing progress during Bucket proposal #21
* [Changed] Core and Blazor Frontend updated to .Net 5.0
* [Changed] File Preview on Import Page now read-only
* [Changed] Misc small visual updates and fixes on Import Page
* [Changed] Consistent Chart Header styles on Report Page
* [Changed] Updated dependencies. Thanks @kekkon
* [Changed] Simplified dependency tree. Thanks @kekkon
* [Changed] Progress calculation for several scenarios #26 #28
* [Fixed] Reworked UI update handling to fix issues on refreshing data #22
* [Fixed] Missing Target Account update for newly created or updated Import Profiles #23
* [Fixed] `MonthOutputConverter.Convert` not using Culture. Thanks @kekkon
* [Fixed] `OpenBudgeteer.Core.Test.ViewModelTest.YearMonthSelectorViewModelTest.Constructor_CheckDefaults` test using thread culture. Thanks @kekkon
* [Fixed] Added Validation checks before saving Bucket data to fix DivideByZeroException #29

### 1.3 (2020-12-15)

* [Add] Support for Sqlite databases #2
* [Add] Unit Tests (not full coverage yet)

### 1.2.1 (2020-12-14)

* [Fixed] Crash on Report Page due to wrong DateTime creation

### 1.2 (2020-10-26)

* [Add] Enable collapse of Bucket Groups
* [Changed] Overall style changes with new font and colors
* [Changed] Newly created Bucket Groups will be created on the first position
* [Changed] Style for Bucket Group modification
* [Fixed] Unable to move newly created Bucket Groups #16

### 1.1.1 (2020-09-07)

* [Fixed] Wrong creation of data for new Rules if the initial selection was used #13
* [Fixed] Missing months for Monthly Bucket Expenses Reports in case of no data #14
* [Fixed] Crashes on Report Page due to display split of Monthly Bucket Expenses Reports #15

### 1.1 (2020-09-05)

* [Add] Added Rule set for automatic Bucket assignments #5
* [Add] Enabled movement of Buckets to other Bucket Groups
* [Add] Enabled movement of Bucket Groups #7
* [Changed] Moved Bucket Movements to Transaction Dialog #1
* [Changed] Opening a new file resets previous Import selection and settings
* [Changed] Optimized Y-Axis Ticks for Month Bucket Expenses on Report Page

### 1.0 (2020-08-10)

* First major release
* Repository now Open Source

### 0.12 (2020-08-04)

* [Add] Current version number and database name to header
* [Add] Bucket Notes
* [Add] Footer for App Icon references
* [Changed] Redesign Page Layout moving Navigation into Header
* [Changed] Proper Home Page with links to Gitlab
* [Changed] Redesign Account Page
* [Changed] Redesign Balance details on Bucket Page
* [Changed] Removed Page Titles
* [Changed] Changed ConnectionString setup for Docker (Splitted full ConnectionString into several pieces for User, Password etc.)
* [Changed] Removed Blazor in Assembly name (now `OpenBudgeteer.dll`)
* [Changed] HTML Title from `OpenBudgeteer.Blazor` to `OpenBudgeteer`
* [Fixed] Database update on Number and Date format for Import Profile
* [Fixed] Total Pending Want update after Bucket Creation and Deletion
* [Fixed] YearMonthSelector styles and alignment on Bucket Page
* [Fixed] Database Issue during Bucket Deletion
* [Fixed] IsInactiveFrom value for newly created Buckets

### 0.11.1 (2020-07-18)

* [Fixed] Broken responsive design for Monthly Bucket Expenses

### 0.11 (2020-07-18)

* [Add] Page with several Reports
* [Add] Popup with Transactions assigned to an Account on Account Page
* [Add] Progress Bar for several Bucket Types
* [Changed] Deleting a Transaction must be confirmed now
* [Changed] Proper Menu Icons on Navigation Menu
* [Changed] Replaced Buttons with proper Menu Icons on several places
* [Changed] Some 0 values are now hidden on Bucket Page
* [Changed] App Name on Navigation Menu
* [Changed] Redesigned File Upload on Import Page
* [Changed] Adapted size of UI elements on Account Page
* [Changed] Adapted layout for Account Creation on Account Page
* [Changed] Accounts can only be closed if Balance is 0
* [Fixed] Wrong Error Dialog on Transaction Page
* [Fixed] Re-enabled handling of inactive Accounts for existing Transactions

### 0.10 (2020-07-12)

* [Add] Preview of final records during data import
* [Add] Options for Date and Number format for data import
* [Add] Header on Bucket Page includes further information like pending Want and negative Balances
* [Add] Popup with Transactions assigned to a Bucket on Bucket Page
* [Add] Message box to confirm successful data import
* [Add] Several Error Message Boxes and proper Error handling
* [Changed] Only display modification buttons on mouse hover
* [Fixed] Inconsistent number output format for 0 on Bucket Page
* [Fixed] Multiple Budget distribution on Buckets

### 0.9 (2020-07-07)

* [Add] Added selection of Delimiter and Text qualifier during data import
* [Add] Button to edit and save all Transaction
* [Add] Bucket Page shows an information at which YearMonth a Bucket will be inactive
* [Changed] Creating a new Bucket uses now current YearMonth as initial "First Target Date"
* [Changed] Creating a new Transaction uses now current YearMonth as initial Transaction Date
* [Changed] A Bucket can only be closed if Balance is 0
* [Fixed] Missing Bucket colors for Crate new Transaction
* [Fixed] Only active Buckets for current YearMonth are displayed for Bucket assignment on Transaction Page

### 0.8 (2020-07-01)

* [Add] Colors for Buckets
* [Add] Button to distribute Budget on Buckets with Want
* [Changed] Input fields for numbers and dates are now properly handled 
* [Fixed] Want calculation for Bucket Type "Monthly expense"

### 0.7 (2020-06-30)

* [Fixed] Pressing Enter for InOut updates UI again
* [Fixed] Creating a new Bucket properly updates UI again
* [Fixed] Fixed Want calculation due to DateTime issues in Data Model

### 0.6 (2020-06-29)

* [Add] Added base implementation for Global Balance details
* [Changed] Optimized Performance for Bucket and Transaction Page
* [Changed] Allow Update of imported Transactions with pending Bucket assignments
* [Fixed] Fixed and optimized Bucket assignment check for Transactions

### 0.5 (2020-06-23)

* [Changed] Switch to Blazor

### 0.4 (2020-05-25)

* [Changed] Basis for generic database handling implemented

### 0.3 (2020-05-19)

* [Changed] Displayed Accounts now sorted by name
* [Changed] Displayed Buckets now sorted by name
* [Changed] Memo no longer mandatory
* [Changed] Manually entered Year triggers only LoadData after pressing Enter
* [Fixed] BucketGroup creation if no group exists
* [Fixed] Transaction Update for Bucket assignments
* [Fixed] Missing database table creation for BucketVersion

### 0.2 (2020-05-18)

* [Add] Enbale creation/update/deletion of Import Profiles

### 0.1 (2020-04-20)

* Initial version
