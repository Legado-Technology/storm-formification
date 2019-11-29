using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using Storm.Formification.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Xunit;

namespace Storm.Formification.Tests
{
    public class DateMonthYearModelBinderTests
    {
        private static DefaultModelBindingContext GetBindingContext(IValueProvider valueProvider, Type modelType)
        {
            var metadataProvider = new EmptyModelMetadataProvider();

            var metadata = metadataProvider.GetMetadataForType(modelType);

            var bindingContext = new DefaultModelBindingContext
            {
                ModelMetadata = metadata,
                ModelName = modelType.Name,
                ModelState = new ModelStateDictionary(),
                ValueProvider = valueProvider,
            };
            return bindingContext;
        }

        [Fact]
        public async Task ShouldNotBindIncompleteValueCollectionForDateTime_MissingMonth()
        {
            var formCollection = new FormCollection(new Dictionary<string, StringValues>()
            {
                { "Year", "1990" },
            });

            var binder = new DateMonthYearModelBinder();

            var vp = new FormValueProvider(BindingSource.Form, formCollection, CultureInfo.CurrentCulture);

            var context = GetBindingContext(vp, typeof(string));

            await binder.BindModelAsync(context);

            context.Result.Model.Should().BeNull();
        }

        [Fact]
        public async Task ShouldNotBindIncompleteValueCollectionForDateTime_MissingYear()
        {
            var formCollection = new FormCollection(new Dictionary<string, StringValues>()
            {
                { "Month", "12" },
            });

            var binder = new DateMonthYearModelBinder();

            var vp = new FormValueProvider(BindingSource.Form, formCollection, CultureInfo.CurrentCulture);

            var context = GetBindingContext(vp, typeof(string));

            await binder.BindModelAsync(context);

            context.Result.Model.Should().BeNull();
        }

        [Theory]
        [InlineData(1990, 1, true, "1990-01")]
        [InlineData(1990, 12, true, "1990-12")]
        [InlineData(1990, 13, false, null)]
        [InlineData(null, null, true, null)]
        public async Task ShouldBindDateTimeAsModel(int? year, int? month, bool isValid, string expectedDateString)
        {
            var formCollection = new FormCollection(new Dictionary<string, StringValues>()
            {
                { "Month", month.ToString() },
                { "Year", year.ToString() },
            });

            var binder = new DateMonthYearModelBinder();

            var vp = new FormValueProvider(BindingSource.Form, formCollection, CultureInfo.CurrentCulture);

            var context = GetBindingContext(vp, typeof(string));

            await binder.BindModelAsync(context);

            context.ModelState.IsValid.Should().Be(isValid);

            if (isValid)
            {
                var dateValueString = (string)context.Result.Model;

                dateValueString.Should().Be(expectedDateString);

                if (!string.IsNullOrWhiteSpace(dateValueString))
                {
                    DateTime.TryParseExact(dateValueString, "yyyy-MM", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, out var dateValue);
                    dateValue.Date.Day.Should().Be(1);
                    dateValue.Date.Month.Should().Be(month);
                    dateValue.Date.Year.Should().Be(year);
                }
            }
            else
            {
                context.ModelState.ErrorCount.Should().Be(1);
            }
        }

        public class CustomModel
        {
            public DateTime? DateField { get; set; }
        }
    }
}
