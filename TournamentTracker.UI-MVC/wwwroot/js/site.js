// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Keep the country initials visible if a remote crest is unavailable.
document.querySelectorAll('.team-logo').forEach(image => {
    const showFallback = () => { image.hidden = true; };
    image.addEventListener('error', showFallback);
    if (image.complete && image.naturalWidth === 0) showFallback();
});
