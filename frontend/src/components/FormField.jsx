export default function FormField({ name, label, errors = {}, hint, multiline = false, ...inputProps }) {
  const fieldErrors = errors[name] || errors[name[0].toUpperCase() + name.slice(1)] || [];
  function localizeBrowserValidation(event) {
    const field = event.currentTarget;
    field.setCustomValidity('');
    if (field.validity.valueMissing) field.setCustomValidity(`${label} alanını doldurun.`);
    else if (field.validity.typeMismatch) field.setCustomValidity(field.type === 'url' ? 'Geçerli bir web adresi girin.' : 'Geçerli bir e-posta adresi girin.');
    else if (field.validity.tooShort) field.setCustomValidity(`${label} en az ${field.minLength} karakter olmalıdır.`);
    else if (!field.validity.valid) field.setCustomValidity('Lütfen geçerli bir değer girin.');
  }
  const Input = multiline ? 'textarea' : 'input';
  return <div className="form-field">
    <label htmlFor={name}>{label}</label>
    <Input id={name} name={name} {...inputProps} aria-invalid={fieldErrors.length > 0}
      onInvalid={localizeBrowserValidation} onInput={(event) => event.currentTarget.setCustomValidity('')}
      aria-describedby={fieldErrors.length ? `${name}-errors` : hint ? `${name}-hint` : undefined} />
    {hint && <p id={`${name}-hint`} className="field-hint">{hint}</p>}
    {fieldErrors.length > 0 && <ul className="field-errors" id={`${name}-errors`}>{fieldErrors.map((error) => <li key={error}>{error}</li>)}</ul>}
  </div>;
}

