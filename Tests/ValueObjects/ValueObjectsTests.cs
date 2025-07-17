using Xunit;
using IntranetDocumentos.Models.ValueObjects;
using System;

namespace IntranetDocumentos.Tests.ValueObjects
{
    public class ValueObjectsTests
    {
        [Fact]
        public void Email_Should_Create_Valid_Email()
        {
            // Arrange
            var emailAddress = "test@example.com";

            // Act
            var email = Email.Create(emailAddress);

            // Assert
            Assert.Equal(emailAddress, email.Value);
            Assert.Equal("example.com", email.GetDomain());
            Assert.Equal("test", email.GetLocalPart());
        }

        [Fact]
        public void Email_Should_Throw_Exception_For_Invalid_Email()
        {
            // Arrange
            var invalidEmail = "invalid-email";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => Email.Create(invalidEmail));
        }

        [Fact]
        public void FileSize_Should_Create_And_Format_Correctly()
        {
            // Arrange
            var bytes = 1024 * 1024; // 1MB

            // Act
            var fileSize = FileSize.FromBytes(bytes);

            // Assert
            Assert.Equal(bytes, fileSize.Bytes);
            Assert.Equal(1, fileSize.Megabytes);
            Assert.Contains("MB", fileSize.ToHumanReadableString());
        }

        [Fact]
        public void FileSize_Should_Support_Arithmetic_Operations()
        {
            // Arrange
            var size1 = FileSize.FromMegabytes(1);
            var size2 = FileSize.FromMegabytes(2);

            // Act
            var sum = size1 + size2;
            var difference = size2 - size1;

            // Assert
            Assert.Equal(3, sum.Megabytes);
            Assert.Equal(1, difference.Megabytes);
        }

        [Fact]
        public void PhoneNumber_Should_Create_And_Format_Brazilian_Phone()
        {
            // Arrange
            var phone = "11987654321";

            // Act
            var phoneNumber = PhoneNumber.Create(phone);

            // Assert
            Assert.Equal(phone, phoneNumber.DigitsOnly);
            Assert.Equal("11", phoneNumber.GetAreaCode());
            Assert.True(phoneNumber.IsMobile());
            Assert.Contains("(11)", phoneNumber.ToFormattedString());
        }

        [Fact]
        public void DocumentChecksum_Should_Create_Valid_Hash()
        {
            // Arrange
            var content = "test content";

            // Act
            var checksum = DocumentChecksum.FromString(content);

            // Assert
            Assert.NotNull(checksum.Value);
            Assert.True(checksum.VerifyData(System.Text.Encoding.UTF8.GetBytes(content)));
        }

        [Fact]
        public void Money_Should_Handle_Currency_Operations()
        {
            // Arrange
            var amount1 = Money.Create(100.50m, "BRL");
            var amount2 = Money.Create(50.25m, "BRL");

            // Act
            var sum = amount1 + amount2;
            var difference = amount1 - amount2;

            // Assert
            Assert.Equal(150.75m, sum.Amount);
            Assert.Equal(50.25m, difference.Amount);
            Assert.Equal("BRL", sum.Currency);
        }

        [Fact]
        public void Money_Should_Throw_Exception_For_Different_Currencies()
        {
            // Arrange
            var brl = Money.Create(100, "BRL");
            var usd = Money.Create(100, "USD");

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => brl + usd);
            Assert.NotNull(exception);
        }

        [Fact]
        public void StatusValue_Should_Validate_Transitions()
        {
            // Arrange
            var draft = StatusValue.Draft;
            var pendingReview = StatusValue.PendingReview;
            var published = StatusValue.Published;

            // Act & Assert
            Assert.True(draft.IsValidTransition(pendingReview));
            Assert.False(draft.IsValidTransition(published));
            Assert.True(draft.CanBeModified());
            Assert.False(published.CanBeModified());
        }

        [Fact]
        public void ValueObjects_Should_Support_Equality()
        {
            // Arrange
            var email1 = Email.Create("test@example.com");
            var email2 = Email.Create("test@example.com");
            var email3 = Email.Create("other@example.com");

            // Act & Assert
            Assert.Equal(email1, email2);
            Assert.NotEqual(email1, email3);
            Assert.True(email1 == email2);
            Assert.False(email1 == email3);
        }

        [Fact]
        public void FileSize_Should_Compare_Correctly()
        {
            // Arrange
            var small = FileSize.FromKilobytes(1);
            var large = FileSize.FromMegabytes(1);

            // Act & Assert
            Assert.True(small < large);
            Assert.True(large > small);
            Assert.True(small <= large);
            Assert.True(large >= small);
        }
    }
}
