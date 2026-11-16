using Microsoft.AspNetCore.Identity;

namespace StajFlow.Api.Configurations;

public class TurkishIdentityErrorDescriber : IdentityErrorDescriber
{
    private static IdentityError Error(string code, string description) => new() { Code = code, Description = description };

    public override IdentityError DefaultError() => Error(nameof(DefaultError), "İşlem tamamlanamadı. Lütfen tekrar deneyin.");
    public override IdentityError ConcurrencyFailure() => Error(nameof(ConcurrencyFailure), "Kayıt başka bir işlem tarafından güncellendi. Lütfen tekrar deneyin.");
    public override IdentityError PasswordMismatch() => Error(nameof(PasswordMismatch), "Parola hatalı.");
    public override IdentityError InvalidToken() => Error(nameof(InvalidToken), "Doğrulama belirteci geçersiz.");
    public override IdentityError RecoveryCodeRedemptionFailed() => Error(nameof(RecoveryCodeRedemptionFailed), "Kurtarma kodu geçersiz veya daha önce kullanılmış.");
    public override IdentityError LoginAlreadyAssociated() => Error(nameof(LoginAlreadyAssociated), "Bu giriş bilgisi başka bir hesapla ilişkilendirilmiş.");
    public override IdentityError InvalidUserName(string? userName) => Error(nameof(InvalidUserName), "Geçerli bir e-posta adresi girin.");
    public override IdentityError InvalidEmail(string? email) => Error(nameof(InvalidEmail), "Geçerli bir e-posta adresi girin.");
    public override IdentityError DuplicateUserName(string userName) => Error(nameof(DuplicateUserName), "Bu e-posta adresiyle zaten bir hesap bulunuyor.");
    public override IdentityError DuplicateEmail(string email) => Error(nameof(DuplicateEmail), "Bu e-posta adresiyle zaten bir hesap bulunuyor.");
    public override IdentityError InvalidRoleName(string? role) => Error(nameof(InvalidRoleName), "Rol adı geçersiz.");
    public override IdentityError DuplicateRoleName(string role) => Error(nameof(DuplicateRoleName), "Bu rol zaten mevcut.");
    public override IdentityError UserAlreadyHasPassword() => Error(nameof(UserAlreadyHasPassword), "Hesabın zaten bir parolası bulunuyor.");
    public override IdentityError UserLockoutNotEnabled() => Error(nameof(UserLockoutNotEnabled), "Bu hesap için kilitleme özelliği etkin değil.");
    public override IdentityError UserAlreadyInRole(string role) => Error(nameof(UserAlreadyInRole), "Kullanıcı zaten bu role sahip.");
    public override IdentityError UserNotInRole(string role) => Error(nameof(UserNotInRole), "Kullanıcı bu role sahip değil.");
    public override IdentityError PasswordTooShort(int length) => Error(nameof(PasswordTooShort), $"Parola en az {length} karakter olmalıdır.");
    public override IdentityError PasswordRequiresUniqueChars(int count) => Error(nameof(PasswordRequiresUniqueChars), $"Parola en az {count} farklı karakter içermelidir.");
    public override IdentityError PasswordRequiresNonAlphanumeric() => Error(nameof(PasswordRequiresNonAlphanumeric), "Parola en az bir simge içermelidir.");
    public override IdentityError PasswordRequiresDigit() => Error(nameof(PasswordRequiresDigit), "Parola en az bir rakam içermelidir.");
    public override IdentityError PasswordRequiresLower() => Error(nameof(PasswordRequiresLower), "Parola en az bir küçük harf içermelidir.");
    public override IdentityError PasswordRequiresUpper() => Error(nameof(PasswordRequiresUpper), "Parola en az bir büyük harf içermelidir.");
}
