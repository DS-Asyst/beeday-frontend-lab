// Reuses the exact same official mechanism the public Home language switcher already posts to
// (POST /culture/set) so an authenticated Settings save reloads through the one server-side
// cookie-writing path instead of a second, parallel one. Interactive Blazor Server components
// have no live HttpContext to write a Set-Cookie header into directly, so the culture/returnUrl
// values are written straight onto the hidden form's inputs here and then the form is submitted
// as a real browser POST — the antiforgery token itself still comes from Razor's own
// <AntiforgeryToken /> markup, untouched by this script.
export function submitCultureSync(formId, culture, returnUrl) {
    const form = document.getElementById(formId);
    if (!form) {
        return;
    }

    const cultureInput = form.querySelector('input[name="culture"]');
    const returnUrlInput = form.querySelector('input[name="returnUrl"]');
    if (cultureInput) {
        cultureInput.value = culture;
    }
    if (returnUrlInput) {
        returnUrlInput.value = returnUrl;
    }

    form.submit();
}
