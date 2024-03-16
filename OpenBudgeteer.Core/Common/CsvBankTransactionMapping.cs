using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.ViewModels.EntityViewModels;
using TinyCsvParser.Mapping;
using TinyCsvParser.TypeConverter;

namespace OpenBudgeteer.Core.Common;

internal class CsvBankTransactionMapping : CsvMapping<ParsedBankTransaction>
{
    private class AmountDecimalConverter : DecimalConverter
    {
        private readonly ImportProfileViewModel _importProfile;

        private AmountDecimalConverter(ImportProfileViewModel importProfile, CultureInfo cultureInfo)
            : base(cultureInfo, NumberStyles.Currency)
        {
            _importProfile = importProfile;
        }

        public static AmountDecimalConverter CreateConverter(ImportProfileViewModel importProfile)
        {
            var culture = string.IsNullOrEmpty(importProfile.NumberFormat)
                ? CultureInfo.CurrentCulture
                : new CultureInfo(importProfile.NumberFormat);
            return new AmountDecimalConverter(importProfile, culture);
        }

        protected override bool InternalConvert(string value, out decimal result)
        {
            if (_importProfile.AdditionalSettingAmountCleanup && !string.IsNullOrEmpty(_importProfile.AdditionalSettingAmountCleanupValue))
                value = Regex.Replace(value, _importProfile.AdditionalSettingAmountCleanupValue, string.Empty);

            return base.InternalConvert(value, out result);
        }
    }
    
    /// <summary>
    /// Definition on how CSV columns should be mapped to <see cref="ImportProfile"/>
    /// </summary>
    /// <remarks>
    /// <see cref="ImportProfile"/> instance and collection of columns will be used to identify columnIndex for
    /// CSV mapping
    /// </remarks>
    /// <param name="importProfile">Instance required for CSV column name</param>
    /// <param name="identifiedColumns">Collection of all CSV columns</param>
    public CsvBankTransactionMapping(ImportProfileViewModel importProfile, IReadOnlyCollection<string> identifiedColumns)
    {
        // Mandatory
        MapProperty(
            identifiedColumns.ToList().IndexOf(importProfile.MemoColumnName!),
            x => x.Memo);
        MapProperty(
            identifiedColumns.ToList().IndexOf(importProfile.TransactionDateColumnName!),
            x => x.TransactionDate, 
            new DateTimeConverter(importProfile.DateFormat));
        
        // Optional
        if (!string.IsNullOrEmpty(importProfile.PayeeColumnName) && importProfile.PayeeColumnName != "---Select Column---")
        {
            MapProperty(
                identifiedColumns.ToList().IndexOf(importProfile.PayeeColumnName), 
                x => x.Payee);
        }

        // Amount Mapping
        switch (importProfile.AdditionalSettingCreditValue)
        {
            // Credit values are in separate columns
            case ImportProfileViewModel.AdditionalSettingsForCreditValues.CreditInSeparateColumns:
                MapUsing((transaction, row) =>
                {
                    if (string.IsNullOrEmpty(importProfile.CreditColumnName)) return false;

                    var debitValue = string.IsNullOrEmpty(importProfile.AmountColumnName)
                        ? null
                        : row.Tokens[identifiedColumns.ToList().IndexOf(importProfile.AmountColumnName)];
                    var creditValue = row.Tokens[identifiedColumns.ToList().IndexOf(importProfile.CreditColumnName)];

                    if (string.IsNullOrWhiteSpace(debitValue) && string.IsNullOrWhiteSpace(creditValue)) return false;


                    var converter = AmountDecimalConverter.CreateConverter(importProfile);
                    converter.TryConvert(debitValue, out var parsedDebitValue);
                    converter.TryConvert(creditValue, out var parsedCreditValue);
                    
                    if (parsedDebitValue > 0)
                    {
                        transaction.Amount = parsedDebitValue;
                    }
                    else
                    {
                        transaction.Amount = parsedCreditValue > 0 ? parsedCreditValue * -1 : parsedCreditValue;
                    }

                    return true;
                });
                break;
            // Debit and Credit values are in the same column but always positive
            case ImportProfileViewModel.AdditionalSettingsForCreditValues.DebitCreditAlwaysPositive:
                MapUsing((transaction, row) =>
                {
                    if (string.IsNullOrEmpty(importProfile.AmountColumnName)) return false;
                    if (string.IsNullOrEmpty(importProfile.CreditColumnIdentifierColumnName)) return false;
                    if (string.IsNullOrEmpty(importProfile.CreditColumnIdentifierValue)) return false;

                    var amountValue = row.Tokens[identifiedColumns.ToList().IndexOf(importProfile.AmountColumnName)];
                    var creditColumnIdentifierValue = 
                        row.Tokens[identifiedColumns.ToList().IndexOf(importProfile.CreditColumnIdentifierColumnName)];

                    if (string.IsNullOrWhiteSpace(amountValue)) return false;
                    
                    var converter = AmountDecimalConverter.CreateConverter(importProfile);
                    converter.TryConvert(amountValue, out var parsedAmountValue);

                    transaction.Amount = creditColumnIdentifierValue == importProfile.CreditColumnIdentifierValue ?
                        parsedAmountValue * -1 :
                        parsedAmountValue;

                    return true;
                });
                break;
            // No special settings for Debit and Credit
            case ImportProfileViewModel.AdditionalSettingsForCreditValues.NoSettings:
            default:
                MapProperty(
                    identifiedColumns.ToList().IndexOf(importProfile.AmountColumnName!),
                    x => x.Amount,
                    AmountDecimalConverter.CreateConverter(importProfile));
                break;
        }
    }
}