tailwind.config = {
    darkMode: "class",
    corePlugins: { preflight: false },
    theme: {
        extend: {
            colors: {
                "secondary": "#795646",
                "secondary-container": "#ffd0bb",
                "primary": "#005f50",
                "on-tertiary-container": "#e7efff",
                "on-primary-fixed": "#00201a",
                "inverse-on-surface": "#f0f1f2",
                "surface": "#f8f9fa",
                "surface-bright": "#f8f9fa",
                "on-primary-container": "#aaffe9",
                "primary-fixed-dim": "#7cd7c1",
                "outline-variant": "#bdc9c4",
                "on-primary": "#ffffff",
                "on-error": "#ffffff",
                "surface-container": "#edeeef",
                "surface-tint": "#006b5b",
                "primary-fixed": "#99f3dd",
                "inverse-surface": "#2e3132",
                "on-secondary-container": "#7a5747",
                "error": "#ba1a1a",
                "surface-container-high": "#e7e8e9",
                "tertiary-fixed": "#d5e3fc",
                "on-error-container": "#93000a",
                "on-surface": "#191c1d",
                "surface-container-low": "#f3f4f5",
                "secondary-fixed-dim": "#eabda9",
                "surface-container-lowest": "#ffffff",
                "tertiary-fixed-dim": "#b9c7df",
                "primary-container": "#0d7a68",
                "on-tertiary-fixed": "#0d1c2e",
                "inverse-primary": "#7cd7c1",
                "surface-variant": "#e1e3e4",
                "on-surface-variant": "#3e4946",
                "on-primary-fixed-variant": "#005144",
                "tertiary-container": "#5f6d82",
                "on-background": "#191c1d",
                "on-secondary-fixed-variant": "#5f3f30",
                "tertiary": "#475569",
                "on-secondary-fixed": "#2d1509",
                "error-container": "#ffdad6",
                "on-tertiary-fixed-variant": "#3a485b",
                "on-secondary": "#ffffff",
                "background": "#f8f9fa",
                "on-tertiary": "#ffffff",
                "surface-container-highest": "#e1e3e4",
                "surface-dim": "#d9dadb",
                "outline": "#6e7a75",
                "secondary-fixed": "#ffdbcc"
            },
            borderRadius: { "DEFAULT": "0.25rem", "lg": "0.5rem", "xl": "0.75rem", "full": "9999px" },
            spacing: {
                "gutter": "1.5rem",
                "space-xl": "2.5rem",
                "space-sm": "0.5rem",
                "space-xs": "0.25rem",
                "margin-mobile": "1rem",
                "gutter-mobile": "1rem",
                "space-md": "1rem",
                "margin": "2rem",
                "space-lg": "1.5rem"
            },
            fontFamily: {
                "label-md": ["Inter"],
                "headline-xl": ["Plus Jakarta Sans"],
                "body-lg": ["Inter"],
                "title-md": ["Plus Jakarta Sans"],
                "headline-md": ["Plus Jakarta Sans"],
                "headline-xl-mobile": ["Plus Jakarta Sans"],
                "headline-lg-mobile": ["Plus Jakarta Sans"],
                "headline-lg": ["Plus Jakarta Sans"],
                "data-tabular": ["Inter"],
                "body-sm": ["Inter"],
                "headline-sm": ["Plus Jakarta Sans"],
                "label-sm": ["Inter"],
                "body-md": ["Inter"]
            },
            fontSize: {
                "label-md": ["13px", { "lineHeight": "16px", "letterSpacing": "0.01em", "fontWeight": "500" }],
                "headline-xl": ["40px", { "lineHeight": "48px", "letterSpacing": "-0.02em", "fontWeight": "700" }],
                "body-lg": ["16px", { "lineHeight": "24px", "fontWeight": "400" }],
                "title-md": ["16px", { "lineHeight": "24px", "fontWeight": "600" }],
                "headline-md": ["24px", { "lineHeight": "32px", "letterSpacing": "-0.01em", "fontWeight": "600" }],
                "headline-xl-mobile": ["30px", { "lineHeight": "38px", "letterSpacing": "-0.015em", "fontWeight": "700" }],
                "headline-lg-mobile": ["26px", { "lineHeight": "34px", "letterSpacing": "-0.01em", "fontWeight": "600" }],
                "headline-lg": ["32px", { "lineHeight": "40px", "letterSpacing": "-0.015em", "fontWeight": "600" }],
                "data-tabular": ["13px", { "lineHeight": "18px", "letterSpacing": "-0.01em", "fontWeight": "500" }],
                "body-sm": ["12px", { "lineHeight": "18px", "fontWeight": "400" }],
                "headline-sm": ["20px", { "lineHeight": "28px", "fontWeight": "600" }],
                "label-sm": ["11px", { "lineHeight": "14px", "letterSpacing": "0.04em", "fontWeight": "600" }],
                "body-md": ["14px", { "lineHeight": "20px", "fontWeight": "400" }]
            }
        }
    }
};

(function () {
    var currentPin = "";
    var maxDigits = 4;

    function pad(n) {
        return String(n).padStart(2, "0");
    }

    function updateClock() {
        var now = new Date();
        var clockEl = document.getElementById("liveClock");
        if (clockEl) {
            clockEl.textContent = pad(now.getHours()) + ":" + pad(now.getMinutes()) + ":" + pad(now.getSeconds());
        }
    }

    function renderDots() {
        for (var i = 1; i <= maxDigits; i++) {
            var dot = document.getElementById("dot-" + i);
            if (!dot) return;
            if (i <= currentPin.length) {
                dot.classList.remove("bg-outline-variant");
                dot.classList.add("bg-primary");
            } else {
                dot.classList.remove("bg-primary");
                dot.classList.add("bg-outline-variant");
            }
        }
    }

    window.pressPin = function (digit) {
        if (currentPin.length < maxDigits) {
            currentPin += digit;
            renderDots();
            var fb = document.getElementById("kioskFeedback");
            if (fb) fb.textContent = "Dígito " + currentPin.length + " de 4 introducido...";

            if (currentPin.length === maxDigits) {
                window.validatePinDirect();
            }
        }
    };

    window.clearPin = function () {
        currentPin = "";
        renderDots();
        var fb = document.getElementById("kioskFeedback");
        if (fb) {
            fb.textContent = "Introduzca su PIN personal de 4 dígitos";
            fb.classList.remove("text-primary", "text-error");
            fb.classList.add("text-on-surface-variant");
        }
    };

    window.validatePinDirect = function () {
        var fb = document.getElementById("kioskFeedback");
        if (!fb) return;
        if (currentPin.length === maxDigits) {
            fb.textContent = "✓ Fichaje Realizado: Dra. Sofía Valdés - Entrada Registrada";
            fb.classList.remove("text-on-surface-variant", "text-error");
            fb.classList.add("text-primary");
            setTimeout(function () {
                window.clearPin();
            }, 2800);
        } else {
            fb.textContent = "Por favor introduzca los 4 dígitos completos";
            fb.classList.remove("text-on-surface-variant", "text-primary");
            fb.classList.add("text-error");
        }
    };

    function start() {
        updateClock();
        setInterval(updateClock, 1000);
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", start);
    } else {
        start();
    }
})();
