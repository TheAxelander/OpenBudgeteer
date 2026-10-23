using System;
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
    /// <summary>
    /// Maximum execution time for user-defined Regex patterns to prevent ReDoS
    /// </summary>
    internal static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Creates the Regex used to clean up amount values. Throws <see cref="ArgumentException"/> for invalid patterns
    /// </summary>
    /// <param name="pattern">User-defined Regex pattern</param>
    /// <returns>Regex instance with execution timeout</returns>
    internal static Regex CreateAmountCleanupRegex(string pattern) =>
        new(pattern, RegexOptions.CultureInvariant, RegexTimeout);

    /// <summary>
    /// Checks if the amount cleanup pattern of the passed <see cref="ImportProfileViewModel"/> is a valid Regex
    /// </summary>
    /// <param name="importProfile">Import Profile to check</param>
    /// <returns>Error message in case of an invalid pattern, otherwise null</returns>
    internal static string? ValidateAmountCleanupPattern(ImportProfileViewModel importProfile)
    {
        if (!importProfile.AdditionalSettingAmountCleanup ||
            string.IsNullOrEmpty(importProfile.AdditionalSettingAmountCleanupValue)) return null;
        try
        {
            CreateAmountCleanupRegex(importProfile.AdditionalSettingAmountCleanupValue);
            return null;
        }
        catch (ArgumentException e)
        {
            return $"Invalid Regex for amount cleanup: {e.Message}";
        }
    }

    private class AmountDecimalConverter : DecimalConverter
    {
        private readonly Regex? _cleanupRegex;

        private AmountDecimalConverter(ImportProfileViewModel importProfile, CultureInfo cultureInfo)
            : base(cultureInfo, NumberStyles.Currency)
        {
            if (importProfile.AdditionalSettingAmountCleanup &&
                !string.IsNullOrEmpty(importProfile.AdditionalSettingAmountCleanupValue))
                _cleanupRegex = CreateAmountCleanupRegex(importProfile.AdditionalSettingAmountCleanupValue);
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
            // RegexMatchTimeoutException is intentionally not caught to abort the whole parsing process
            if (_cleanupRegex is not null) value = _cleanupRegex.Replace(value, string.Empty);

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
            identifiedColumns.ToList().IndexOf(importProfile.MemoColumnName),
            x => x.Memo);
        MapProperty(
            identifiedColumns.ToList().IndexOf(importProfile.TransactionDateColumnName),
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
