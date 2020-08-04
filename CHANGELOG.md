### 0.12 (2020-08-04)

* [Add] Current version number and database name to header #43
* [Add] Bucket Notes #51
* [Add] Footer for App Icon references
* [Changed] Redesign Page Layout moving Navigation into Header
* [Changed] Proper Home Page with links to Gitlab #40
* [Changed] Redesign Account Page
* [Changed] Redesign Balance details on Bucket Page #50
* [Changed] Removed Page Titles
* [Changed] Changed ConnectionString setup for Docker (Splitted full ConnectionString into several pieces for User, Password etc.)
* [Changed] Removed Blazor in Assembly name (now ```OpenBudgeteer.dll```)
* [Changed] HTML Title from ```OpenBudgeteer.Blazor``` to ```OpenBudgeteer```
* [Fixed] Database update on Number and Date format for Import Profile #47 
* [Fixed] Total Pending Want update after Bucket Creation and Deletion #52
* [Fixed] YearMonthSelector styles and alignment on Bucket Page
* [Fixed] Database Issue during Bucket Deletion
* [Fixed] IsInactiveFrom value for newly created Buckets

### 0.11.1 (2020-07-18)

* [Fixed] Broken responsive design for Monthly Bucket Expenses #42

### 0.11 (2020-07-18)

* [Add] Page with several Reports #1
* [Add] Popup with Transactions assigned to an Account on Account Page #38
* [Add] Progress Bar for several Bucket Types #20
* [Changed] Deleting a Transaction must be confirmed now #17
* [Changed] Proper Menu Icons on Navigation Menu #33
* [Changed] Replaced Buttons with proper Menu Icons on several places #39
* [Changed] Some 0 values are now hidden on Bucket Page #41
* [Changed] App Name on Navigation Menu
* [Changed] Redesigned File Upload on Import Page
* [Changed] Adapted size of UI elements on Account Page
* [Changed] Adapted layout for Account Creation on Account Page
* [Changed] Accounts can only be closed if Balance is 0
* [Fixed] Wrong Error Dialog on Transaction Page
* [Fixed] Re-enabled handling of inactive Accounts for existing Transactions

### 0.10 (2020-07-12)

* [Add] Preview of final records during data import #23
* [Add] Options for Date and Number format for data import #8
* [Add] Header on Bucket Page includes further information like pending Want and negative Balances #35
* [Add] Popup with Transactions assigned to a Bucket on Bucket Page #19
* [Add] Message box to confirm successful data import #22
* [Add] Several Error Message Boxes and proper Error handling #2
* [Changed] Only display modification buttons on mouse hover #16
* [Fixed] Inconsistent number output format for 0 on Bucket Page #29
* [Fixed] Multiple Budget distribution on Buckets #28

### 0.9 (2020-07-07)

* [Add] Added selection of Delimiter and Text qualifier during data import #8
* [Add] Button to edit and save all Transaction #30
* [Add] Bucket Page shows an information at which YearMonth a Bucket will be inactive #31
* [Changed] Creating a new Bucket uses now current YearMonth as initial "First Target Date" #25
* [Changed] Creating a new Transaction uses now current YearMonth as initial Transaction Date
* [Changed] A Bucket can only be closed if Balance is 0 #32
* [Fixed] Missing Bucket colors for Crate new Transaction #27
* [Fixed] Only active Buckets for current YearMonth are displayed for Bucket assignment on Transaction Page #26

### 0.8 (2020-07-01)

* [Add] Colors for Buckets #14
* [Add] Button to distribute Budget on Buckets with Want #11
* [Changed] Input fields for numbers and dates are now properly handled #15
* [Fixed] Want calculation for Bucket Type "Monthly expense" #13

### 0.7 (2020-06-30)

* [Fixed] Pressing Enter for InOut updates UI again #12
* [Fixed] Creating a new Bucket properly updates UI again #9
* [Fixed] Fixed Want calculation due to DateTime issues in Data Model #10

### 0.6 (2020-06-29)

* [Add] Added base implementation for Global Balance details
* [Changed] Optimized Performance for Bucket and Transaction Page #4
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

* [Add] Enbale creation/update/deletion of Import Profiles #5

### 0.1 (2020-04-20)

* Initial version
