### 1.6 (20xx-xx-xx)

* [Add] Enhanced Bucket assignment for Bank Transaction (display remaining amount, manual triggered split) Thanks [ambroser1971](https://github.com/ambroser1971)
* [Add] Recurring Transactions [#74](https://github.com/TheAxelander/OpenBudgeteer/issues/74)
* [Add] Page with reports checking consistency of database data [#3](https://github.com/TheAxelander/OpenBudgeteer/issues/3)
* [Add] Usage of themes from [Bootswatch](https://bootswatch.com) [#101](https://github.com/TheAxelander/OpenBudgeteer/issues/101)
* [Add] Enable MySql User and Database creation via MySql root User [#75](https://github.com/TheAxelander/OpenBudgeteer/issues/75)
* [Add] Bucket Details with Statistics [#20](https://github.com/TheAxelander/OpenBudgeteer/issues/20)
* [Breaking Change] Docker Environment Variable Format [#71](https://github.com/TheAxelander/OpenBudgeteer/issues/71)
* [Changed] Bucket Page: Want, In and Activity figures also now on Bucket Group level [#99](https://github.com/TheAxelander/OpenBudgeteer/issues/99)
* [Changed] Updated Bootstrap to v5.1.3
* [Changed] Replaced Open Iconic icons with Bootstrap Icons
* [Changed] Made Icons8 images offline available 
* [Fixed] App Startup will now wait for availability of MySql database [#50](https://github.com/TheAxelander/OpenBudgeteer/issues/50)
* [Fixed] Rules disappear after Cancel All [#102](https://github.com/TheAxelander/OpenBudgeteer/issues/102)
* [Fixed] Correct number of months shown on Report Page (e.g. should show past 24 months but displays 25 months)

### 1.5.2 (2022-03-26)

* [Fixed] Sqlite database lock while saving multiple Bank Transaction [#90](https://github.com/TheAxelander/OpenBudgeteer/issues/90)

### 1.5.1 (2022-02-26)

* [Fixed] Amount conversion with currency characters. [#82](https://github.com/TheAxelander/OpenBudgeteer/issues/82) [#83](https://github.com/TheAxelander/OpenBudgeteer/pull/83) Thanks [Hazy87](https://github.com/Hazy87)
* [Fixed] Amount conversion with 0 values. [#72](https://github.com/TheAxelander/OpenBudgeteer/issues/72)

### 1.5 (2022-02-19)

* [Add] Option to set Localization [#52](https://github.com/TheAxelander/OpenBudgeteer/issues/52)
* [Add] Enable mapping of seperated columns for Debit and Credit Amount on Import Page [#53](https://github.com/TheAxelander/OpenBudgeteer/issues/53)
* [Add] Duplicate Check before importing data with an option to exclude certain records [#49](https://github.com/TheAxelander/OpenBudgeteer/issues/49)
* [Fixed] Moved Sqlite database path back to `/app/database` [#63](https://github.com/TheAxelander/OpenBudgeteer/issues/63)
* [Fixed] Crash on Rules Page in case a Bucket has been deleted with an existing RuleSet [#65](https://github.com/TheAxelander/OpenBudgeteer/issues/65)
* [Fixed] Include Transactions which are in modification in all filters to prevent immediate disappearance [#67](https://github.com/TheAxelander/OpenBudgeteer/issues/67)

### 1.4.1 (2021-11-28)

* [Changed] Handling of Bucket Group creation (fixes also crashes during creation cancellation [#56](https://github.com/TheAxelander/OpenBudgeteer/issues/56))
* [Fixed] Unable to add multiple Buckets during Bank Transaction creation [#55](https://github.com/TheAxelander/OpenBudgeteer/issues/55)
* [Fixed] Crash on Report Page using sqlite [#57](https://github.com/TheAxelander/OpenBudgeteer/issues/57)

### 1.4 (2021-11-14)

* [Add] Info Dialog during Bucket proposal and optimized proposal performance [#21](https://github.com/TheAxelander/OpenBudgeteer/issues/21)
* [Add] Filter on Transaction Page [#25](https://github.com/TheAxelander/OpenBudgeteer/issues/25)
* [Add] On Import Page final message box shows an option to clear the form [#45](https://github.com/TheAxelander/OpenBudgeteer/issues/45)
* [Changed] Core and Blazor Frontend updated to .Net 6.0
* [Changed] File Preview on Import Page now read-only
* [Changed] Misc small visual updates and fixes on Import Page
* [Changed] Consistent Chart Header styles on Report Page
* [Changed] Updated dependencies. Thanks @kekkon
* [Changed] Simplified dependency tree. Thanks @kekkon
* [Changed] Progress calculation for several scenarios [#26](https://github.com/TheAxelander/OpenBudgeteer/issues/26) [#28](https://github.com/TheAxelander/OpenBudgeteer/issues/28)
* [Changed] Links and text due to switch to Github [#46](https://github.com/TheAxelander/OpenBudgeteer/issues/46) [#47](https://github.com/TheAxelander/OpenBudgeteer/issues/47)
* [Fixed] Reworked UI update handling to fix issues on refreshing data [#22](https://github.com/TheAxelander/OpenBudgeteer/issues/22)
* [Fixed] Missing Target Account update for newly created or updated Import Profiles [#23](https://github.com/TheAxelander/OpenBudgeteer/issues/23)
* [Fixed] `MonthOutputConverter.Convert` not using Culture. Thanks [kekkon](https://gitlab.com/kekkon)
* [Fixed] `OpenBudgeteer.Core.Test.ViewModelTest.YearMonthSelectorViewModelTest.Constructor_CheckDefaults` test using thread culture. Thanks [kekkon](https://gitlab.com/kekkon)
* [Fixed] Added Validation checks before saving Bucket data to fix DivideByZeroException [#29](https://github.com/TheAxelander/OpenBudgeteer/issues/29)
* [Fixed] Trigger of `SelectedYearMonthChanged` passing `OpenBudgeteer.Core.Test.ViewModelTest.SelectedYearMonthChanged_CheckEventHasBeenInvoked` Test
* [Fixed] Wrong text in confirmation message box for deleting a Rule [#44](https://github.com/TheAxelander/OpenBudgeteer/issues/44)

### 1.3 (2020-12-15)

* [Add] Support for Sqlite databases [#2](https://github.com/TheAxelander/OpenBudgeteer/issues/2)
* [Add] Unit Tests (not full coverage yet)

### 1.2.1 (2020-12-14)

* [Fixed] Crash on Report Page due to wrong DateTime creation

### 1.2 (2020-10-26)

* [Add] Enable collapse of Bucket Groups
* [Changed] Overall style changes with new font and colors
* [Changed] Newly created Bucket Groups will be created on the first position
* [Changed] Style for Bucket Group modification
* [Fixed] Unable to move newly created Bucket Groups [#16](https://github.com/TheAxelander/OpenBudgeteer/issues/16)

### 1.1.1 (2020-09-07)

* [Fixed] Wrong creation of data for new Rules if the initial selection was used [#13](https://github.com/TheAxelander/OpenBudgeteer/issues/13)
* [Fixed] Missing months for Monthly Bucket Expenses Reports in case of no data [#14](https://github.com/TheAxelander/OpenBudgeteer/issues/14)
* [Fixed] Crashes on Report Page due to display split of Monthly Bucket Expenses Reports [#15](https://github.com/TheAxelander/OpenBudgeteer/issues/15)

### 1.1 (2020-09-05)

* [Add] Added Rule set for automatic Bucket assignments [#5](https://github.com/TheAxelander/OpenBudgeteer/issues/5)
* [Add] Enabled movement of Buckets to other Bucket Groups
* [Add] Enabled movement of Bucket Groups [#7](https://github.com/TheAxelander/OpenBudgeteer/issues/7)
* [Changed] Moved Bucket Movements to Transaction Dialog [#1](https://github.com/TheAxelander/OpenBudgeteer/issues/1)
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
