/* @ds-bundle: {"format":4,"namespace":"GarageBoxDesignSystem_0843eb","components":[{"name":"AppHeader","sourcePath":"components/app/AppHeader.jsx"},{"name":"DocumentChip","sourcePath":"components/app/DocumentChip.jsx"},{"name":"EmptyState","sourcePath":"components/app/EmptyState.jsx"},{"name":"MaintenanceRow","sourcePath":"components/app/MaintenanceRow.jsx"},{"name":"ObligationRow","sourcePath":"components/app/ObligationRow.jsx"},{"name":"PlanCard","sourcePath":"components/app/PlanCard.jsx"},{"name":"SectionHeader","sourcePath":"components/app/SectionHeader.jsx"},{"name":"SsoButton","sourcePath":"components/app/SsoButton.jsx"},{"name":"StatTile","sourcePath":"components/app/StatTile.jsx"},{"name":"TabBar","sourcePath":"components/app/TabBar.jsx"},{"name":"VehicleCard","sourcePath":"components/app/VehicleCard.jsx"},{"name":"Badge","sourcePath":"components/core/Badge.jsx"},{"name":"Button","sourcePath":"components/core/Button.jsx"},{"name":"Card","sourcePath":"components/core/Card.jsx"},{"name":"Checkbox","sourcePath":"components/core/Checkbox.jsx"},{"name":"ICONS","sourcePath":"components/core/Icon.jsx"},{"name":"ICON_NAMES","sourcePath":"components/core/Icon.jsx"},{"name":"Icon","sourcePath":"components/core/Icon.jsx"},{"name":"IconButton","sourcePath":"components/core/IconButton.jsx"},{"name":"Input","sourcePath":"components/core/Input.jsx"},{"name":"ListRow","sourcePath":"components/core/ListRow.jsx"},{"name":"ProgressBar","sourcePath":"components/core/ProgressBar.jsx"},{"name":"SegmentedControl","sourcePath":"components/core/SegmentedControl.jsx"},{"name":"Select","sourcePath":"components/core/Select.jsx"},{"name":"Switch","sourcePath":"components/core/Switch.jsx"}],"sourceHashes":{"components/app/AppHeader.jsx":"c83544ae6fc2","components/app/DocumentChip.jsx":"ff769fc2dfa3","components/app/EmptyState.jsx":"ac84ba619f7e","components/app/MaintenanceRow.jsx":"1ae953151066","components/app/ObligationRow.jsx":"4a1396b0bf40","components/app/PlanCard.jsx":"c9fc48b5efb6","components/app/SectionHeader.jsx":"547bfc309dda","components/app/SsoButton.jsx":"51177b7d1cd4","components/app/StatTile.jsx":"4445104b48a9","components/app/TabBar.jsx":"f9e928a21120","components/app/VehicleCard.jsx":"81ef578762f1","components/core/Badge.jsx":"e5d80e954455","components/core/Button.jsx":"2351f93702c8","components/core/Card.jsx":"13aa3a0d8081","components/core/Checkbox.jsx":"352417f1456f","components/core/Icon.jsx":"062108ffc81d","components/core/IconButton.jsx":"879722127ff5","components/core/Input.jsx":"ad954f2928fc","components/core/ListRow.jsx":"12ecee84eec8","components/core/ProgressBar.jsx":"3807cbb25479","components/core/SegmentedControl.jsx":"d7557dde4a94","components/core/Select.jsx":"928e3fbaa0b2","components/core/Switch.jsx":"c865a01a0ba5","ui_kits/mobile-app/add-car-screen.jsx":"082fea60479e","ui_kits/mobile-app/auth-screen.jsx":"4921b4463179","ui_kits/mobile-app/dashboard-screen.jsx":"06c740d3d2ea","ui_kits/mobile-app/onboarding-screen.jsx":"97e47c58c0cc","ui_kits/mobile-app/phone-shell.jsx":"b99d31c4231b","ui_kits/mobile-app/subscription-screen.jsx":"7cf309a2363a"},"inlinedExternals":[],"unexposedExports":[]} */

(() => {

const __ds_ns = (window.GarageBoxDesignSystem_0843eb = window.GarageBoxDesignSystem_0843eb || {});

const __ds_scope = {};

(__ds_ns.__errors = __ds_ns.__errors || []);

// components/app/SectionHeader.jsx
try { (() => {
function SectionHeader({
  title,
  action,
  onAction,
  style
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'baseline',
      justifyContent: 'space-between',
      gap: 12,
      marginBottom: 10,
      ...style
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 'var(--text-micro)',
      fontWeight: 'var(--weight-semibold)',
      letterSpacing: 'var(--track-caps)',
      textTransform: 'uppercase',
      color: 'var(--text-faint)'
    }
  }, title), action ? /*#__PURE__*/React.createElement("button", {
    type: "button",
    onClick: onAction,
    style: {
      background: 'none',
      border: 'none',
      padding: 0,
      cursor: 'pointer',
      color: 'var(--accent-text)',
      fontFamily: 'var(--font-body)',
      fontSize: 'var(--text-body-sm)',
      fontWeight: 'var(--weight-semibold)'
    }
  }, action) : null);
}
Object.assign(__ds_scope, { SectionHeader });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/app/SectionHeader.jsx", error: String((e && e.message) || e) }); }

// components/app/SsoButton.jsx
try { (() => {
const {
  useState
} = React;
const MARKS = {
  google: {
    label: 'Continue with Google',
    mark: /*#__PURE__*/React.createElement("svg", {
      width: "18",
      height: "18",
      viewBox: "0 0 18 18",
      "aria-hidden": "true"
    }, /*#__PURE__*/React.createElement("path", {
      fill: "#4285F4",
      d: "M17.64 9.2c0-.64-.06-1.25-.16-1.84H9v3.48h4.84a4.14 4.14 0 0 1-1.8 2.72v2.26h2.92c1.7-1.57 2.68-3.88 2.68-6.62z"
    }), /*#__PURE__*/React.createElement("path", {
      fill: "#34A853",
      d: "M9 18c2.43 0 4.47-.8 5.96-2.18l-2.92-2.26c-.8.54-1.84.86-3.04.86-2.34 0-4.32-1.58-5.03-3.7H.96v2.33A9 9 0 0 0 9 18z"
    }), /*#__PURE__*/React.createElement("path", {
      fill: "#FBBC05",
      d: "M3.97 10.72a5.4 5.4 0 0 1 0-3.44V4.96H.96a9 9 0 0 0 0 8.08l3-2.32z"
    }), /*#__PURE__*/React.createElement("path", {
      fill: "#EA4335",
      d: "M9 3.58c1.32 0 2.5.45 3.44 1.35l2.58-2.58C13.46.89 11.43 0 9 0A9 9 0 0 0 .96 4.96l3.01 2.32C4.68 5.16 6.66 3.58 9 3.58z"
    }))
  },
  apple: {
    label: 'Continue with Apple',
    mark: /*#__PURE__*/React.createElement("svg", {
      width: "18",
      height: "18",
      viewBox: "0 0 18 18",
      "aria-hidden": "true"
    }, /*#__PURE__*/React.createElement("path", {
      fill: "currentColor",
      d: "M13.6 9.55c.02 2.32 2.03 3.09 2.06 3.1-.02.06-.32 1.1-1.06 2.17-.64.93-1.3 1.85-2.35 1.87-1.03.02-1.36-.61-2.53-.61-1.18 0-1.55.6-2.52.63-1.01.04-1.78-1-2.43-1.93-1.33-1.9-2.34-5.38-.98-7.72a3.79 3.79 0 0 1 3.2-1.94c.99-.02 1.93.66 2.53.66.6 0 1.75-.82 2.94-.7.5.02 1.91.2 2.81 1.52-.07.05-1.68.98-1.67 2.95zM11.7 3.2c.54-.65.9-1.55.8-2.45-.77.03-1.71.51-2.26 1.16-.5.57-.93 1.5-.81 2.38.86.07 1.74-.44 2.28-1.09z"
    }))
  },
  facebook: {
    label: 'Continue with Facebook',
    mark: /*#__PURE__*/React.createElement("svg", {
      width: "18",
      height: "18",
      viewBox: "0 0 18 18",
      "aria-hidden": "true"
    }, /*#__PURE__*/React.createElement("path", {
      fill: "#1877F2",
      d: "M18 9a9 9 0 1 0-10.41 8.89v-6.29H5.31V9h2.28V7.02c0-2.25 1.34-3.5 3.4-3.5.98 0 2.01.18 2.01.18v2.21h-1.13c-1.12 0-1.47.7-1.47 1.4V9h2.5l-.4 2.6h-2.1v6.29A9 9 0 0 0 18 9z"
    }))
  }
};
function SsoButton({
  provider = 'google',
  label,
  onClick,
  style
}) {
  const [hover, setHover] = useState(false);
  const p = MARKS[provider] || MARKS.google;
  return /*#__PURE__*/React.createElement("button", {
    type: "button",
    onClick: onClick,
    onMouseEnter: () => setHover(true),
    onMouseLeave: () => setHover(false),
    style: {
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      gap: 10,
      width: '100%',
      height: 48,
      background: hover ? 'var(--surface-hover)' : 'var(--surface-raised)',
      border: '1px solid var(--border-subtle)',
      borderRadius: 'var(--radius-md)',
      color: 'var(--text-strong)',
      fontFamily: 'var(--font-body)',
      fontSize: 'var(--text-body-md)',
      fontWeight: 'var(--weight-semibold)',
      cursor: 'pointer',
      transition: 'background var(--dur-fast) var(--ease-out)',
      ...style
    }
  }, p.mark, label || p.label);
}
Object.assign(__ds_scope, { SsoButton });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/app/SsoButton.jsx", error: String((e && e.message) || e) }); }

// components/core/Card.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
function Card({
  children,
  padding = 16,
  tone = 'default',
  interactive = false,
  style,
  ...rest
}) {
  const bg = tone === 'raised' ? 'var(--surface-raised)' : tone === 'accent' ? 'var(--accent-tint)' : 'var(--surface-card)';
  return /*#__PURE__*/React.createElement("div", _extends({
    style: {
      background: bg,
      border: '1px solid ' + (tone === 'accent' ? 'rgba(91,140,255,0.35)' : 'var(--border-subtle)'),
      borderRadius: 'var(--radius-lg)',
      padding,
      boxShadow: 'var(--shadow-card)',
      cursor: interactive ? 'pointer' : undefined,
      transition: 'background var(--dur-fast) var(--ease-out), border-color var(--dur-fast) var(--ease-out)',
      ...style
    }
  }, rest), children);
}
Object.assign(__ds_scope, { Card });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/Card.jsx", error: String((e && e.message) || e) }); }

// components/core/Icon.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
// Lucide icons (ISC), copied from github.com/lucide-icons/lucide and inlined so the
// component works from any page depth without a sprite fetch.
const ICONS = {
  "arrow-right": "<path d=\"M5 12h14\"></path> <path d=\"m12 5 7 7-7 7\"></path>",
  "badge-check": "<path d=\"M3.85 8.62a4 4 0 0 1 4.78-4.77 4 4 0 0 1 6.74 0 4 4 0 0 1 4.78 4.78 4 4 0 0 1 0 6.74 4 4 0 0 1-4.77 4.78 4 4 0 0 1-6.75 0 4 4 0 0 1-4.78-4.77 4 4 0 0 1 0-6.76Z\"></path> <path d=\"m9 12 2 2 4-4\"></path>",
  "battery": "<path d=\"M 22 14 L 22 10\"></path> <rect x=\"2\" y=\"6\" width=\"16\" height=\"12\" rx=\"2\"></rect>",
  "bell-ring": "<path d=\"M10.268 21a2 2 0 0 0 3.464 0\"></path> <path d=\"M22 8c0-2.3-.8-4.3-2-6\"></path> <path d=\"M3.262 15.326A1 1 0 0 0 4 17h16a1 1 0 0 0 .74-1.673C19.41 13.956 18 12.499 18 8A6 6 0 0 0 6 8c0 4.499-1.411 5.956-2.738 7.326\"></path> <path d=\"M4 2C2.8 3.7 2 5.7 2 8\"></path>",
  "bell": "<path d=\"M10.268 21a2 2 0 0 0 3.464 0\"></path> <path d=\"M3.262 15.326A1 1 0 0 0 4 17h16a1 1 0 0 0 .74-1.673C19.41 13.956 18 12.499 18 8A6 6 0 0 0 6 8c0 4.499-1.411 5.956-2.738 7.326\"></path>",
  "calendar": "<path d=\"M8 2v3\"></path> <path d=\"M16 2v3\"></path> <rect x=\"3\" y=\"3\" width=\"18\" height=\"18\" rx=\"2\"></rect> <path d=\"M3 9h18\"></path>",
  "camera": "<path d=\"M13.997 4a2 2 0 0 1 1.76 1.05l.486.9A2 2 0 0 0 18.003 7H20a2 2 0 0 1 2 2v9a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V9a2 2 0 0 1 2-2h1.997a2 2 0 0 0 1.759-1.048l.489-.904A2 2 0 0 1 10.004 4z\"></path> <circle cx=\"12\" cy=\"13\" r=\"3\"></circle>",
  "car-front": "<path d=\"m21 8-2 2-1.5-3.7A2 2 0 0 0 15.646 5H8.4a2 2 0 0 0-1.903 1.257L5 10 3 8\"></path> <path d=\"M7 14h.01\"></path> <path d=\"M17 14h.01\"></path> <rect width=\"18\" height=\"8\" x=\"3\" y=\"10\" rx=\"2\"></rect> <path d=\"M5 18v2\"></path> <path d=\"M19 18v2\"></path>",
  "car": "<path d=\"M19 17h2c.6 0 1-.4 1-1v-3c0-.9-.7-1.7-1.5-1.9C18.7 10.6 16 10 16 10s-1.3-1.4-2.2-2.3c-.5-.4-1.1-.7-1.8-.7H5c-.6 0-1.1.4-1.4.9l-1.4 2.9A3.7 3.7 0 0 0 2 12v4c0 .6.4 1 1 1h2\"></path> <circle cx=\"7\" cy=\"17\" r=\"2\"></circle> <path d=\"M9 17h6\"></path> <circle cx=\"17\" cy=\"17\" r=\"2\"></circle>",
  "check": "<path d=\"M20 6 9 17l-5-5\"></path>",
  "chevron-left": "<path d=\"m15 18-6-6 6-6\"></path>",
  "chevron-right": "<path d=\"m9 18 6-6-6-6\"></path>",
  "chevrons-up-down": "<path d=\"m7 15 5 5 5-5\"></path> <path d=\"m7 9 5-5 5 5\"></path>",
  "circle-alert": "<circle cx=\"12\" cy=\"12\" r=\"10\"></circle> <line x1=\"12\" x2=\"12\" y1=\"8\" y2=\"12\"></line> <line x1=\"12\" x2=\"12.01\" y1=\"16\" y2=\"16\"></line>",
  "circle-check": "<circle cx=\"12\" cy=\"12\" r=\"10\"></circle> <path d=\"m9 12 2 2 4-4\"></path>",
  "clock": "<circle cx=\"12\" cy=\"12\" r=\"10\"></circle> <path d=\"M12 6v6l4 2\"></path>",
  "credit-card": "<rect width=\"20\" height=\"14\" x=\"2\" y=\"5\" rx=\"2\"></rect> <line x1=\"2\" x2=\"22\" y1=\"10\" y2=\"10\"></line>",
  "disc": "<circle cx=\"12\" cy=\"12\" r=\"10\"></circle> <circle cx=\"12\" cy=\"12\" r=\"2\"></circle>",
  "droplet": "<path d=\"M12 22a7 7 0 0 0 7-7c0-2-1-3.9-3-5.5s-3.5-4-4-6.5c-.5 2.5-2 4.9-4 6.5C6 11.1 5 13 5 15a7 7 0 0 0 7 7z\"></path>",
  "eye-off": "<path d=\"M10.733 5.076a10.744 10.744 0 0 1 11.205 6.575 1 1 0 0 1 0 .696 10.747 10.747 0 0 1-1.444 2.49\"></path> <path d=\"M14.084 14.158a3 3 0 0 1-4.242-4.242\"></path> <path d=\"M17.479 17.499a10.75 10.75 0 0 1-15.417-5.151 1 1 0 0 1 0-.696 10.75 10.75 0 0 1 4.446-5.143\"></path> <path d=\"m2 2 20 20\"></path>",
  "eye": "<path d=\"M2.062 12.348a1 1 0 0 1 0-.696 10.75 10.75 0 0 1 19.876 0 1 1 0 0 1 0 .696 10.75 10.75 0 0 1-19.876 0\"></path> <circle cx=\"12\" cy=\"12\" r=\"3\"></circle>",
  "file-text": "<path d=\"M6 22a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h8a2.4 2.4 0 0 1 1.704.706l3.588 3.588A2.4 2.4 0 0 1 20 8v12a2 2 0 0 1-2 2z\"></path> <path d=\"M14 2v5a1 1 0 0 0 1 1h5\"></path> <path d=\"M10 9H8\"></path> <path d=\"M16 13H8\"></path> <path d=\"M16 17H8\"></path>",
  "folder": "<path d=\"M20 20a2 2 0 0 0 2-2V8a2 2 0 0 0-2-2h-7.9a2 2 0 0 1-1.69-.9L9.6 3.9A2 2 0 0 0 7.93 3H4a2 2 0 0 0-2 2v13a2 2 0 0 0 2 2Z\"></path>",
  "fuel": "<path d=\"M14 13h2a2 2 0 0 1 2 2v2a2 2 0 0 0 4 0v-6.998a2 2 0 0 0-.59-1.42L18 5\"></path> <path d=\"M14 21V5a2 2 0 0 0-2-2H5a2 2 0 0 0-2 2v16\"></path> <path d=\"M2 21h13\"></path> <path d=\"M3 9h11\"></path>",
  "funnel": "<path d=\"M10 20a1 1 0 0 0 .553.895l2 1A1 1 0 0 0 14 21v-7a2 2 0 0 1 .517-1.341L21.74 4.67A1 1 0 0 0 21 3H3a1 1 0 0 0-.742 1.67l7.225 7.989A2 2 0 0 1 10 14z\"></path>",
  "gauge": "<path d=\"m12 14 4-4\"></path> <path d=\"M3.34 19a10 10 0 1 1 17.32 0\"></path>",
  "house": "<path d=\"M15 21v-8a1 1 0 0 0-1-1h-4a1 1 0 0 0-1 1v8\"></path> <path d=\"M3 10a2 2 0 0 1 .709-1.528l7-6a2 2 0 0 1 2.582 0l7 6A2 2 0 0 1 21 10v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z\"></path>",
  "lock": "<rect width=\"18\" height=\"11\" x=\"3\" y=\"11\" rx=\"2\" ry=\"2\"></rect> <path d=\"M7 11V7a5 5 0 0 1 10 0v4\"></path>",
  "mail": "<path d=\"m22 7-8.991 5.727a2 2 0 0 1-2.009 0L2 7\"></path> <rect x=\"2\" y=\"4\" width=\"20\" height=\"16\" rx=\"2\"></rect>",
  "menu": "<path d=\"M4 5h16\"></path> <path d=\"M4 12h16\"></path> <path d=\"M4 19h16\"></path>",
  "paperclip": "<path d=\"m16 6-8.414 8.586a2 2 0 0 0 2.829 2.829l8.414-8.586a4 4 0 1 0-5.657-5.657l-8.379 8.551a6 6 0 1 0 8.485 8.485l8.379-8.551\"></path>",
  "pencil": "<path d=\"M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z\"></path> <path d=\"m15 5 4 4\"></path>",
  "plus": "<path d=\"M5 12h14\"></path> <path d=\"M12 5v14\"></path>",
  "receipt": "<path d=\"M12 17V7\"></path> <path d=\"M16 8h-6a2 2 0 0 0 0 4h4a2 2 0 0 1 0 4H8\"></path> <path d=\"M4 3a1 1 0 0 1 1-1 1.3 1.3 0 0 1 .7.2l.933.6a1.3 1.3 0 0 0 1.4 0l.934-.6a1.3 1.3 0 0 1 1.4 0l.933.6a1.3 1.3 0 0 0 1.4 0l.933-.6a1.3 1.3 0 0 1 1.4 0l.934.6a1.3 1.3 0 0 0 1.4 0l.933-.6A1.3 1.3 0 0 1 19 2a1 1 0 0 1 1 1v18a1 1 0 0 1-1 1 1.3 1.3 0 0 1-.7-.2l-.933-.6a1.3 1.3 0 0 0-1.4 0l-.934.6a1.3 1.3 0 0 1-1.4 0l-.933-.6a1.3 1.3 0 0 0-1.4 0l-.933.6a1.3 1.3 0 0 1-1.4 0l-.934-.6a1.3 1.3 0 0 0-1.4 0l-.933.6a1.3 1.3 0 0 1-.7.2 1 1 0 0 1-1-1z\"></path>",
  "scan-line": "<path d=\"M3 7V5a2 2 0 0 1 2-2h2\"></path> <path d=\"M17 3h2a2 2 0 0 1 2 2v2\"></path> <path d=\"M21 17v2a2 2 0 0 1-2 2h-2\"></path> <path d=\"M7 21H5a2 2 0 0 1-2-2v-2\"></path> <path d=\"M7 12h10\"></path>",
  "search": "<path d=\"m21 21-4.34-4.34\"></path> <circle cx=\"11\" cy=\"11\" r=\"8\"></circle>",
  "settings": "<path d=\"M9.671 4.136a2.34 2.34 0 0 1 4.659 0 2.34 2.34 0 0 0 3.319 1.915 2.34 2.34 0 0 1 2.33 4.033 2.34 2.34 0 0 0 0 3.831 2.34 2.34 0 0 1-2.33 4.033 2.34 2.34 0 0 0-3.319 1.915 2.34 2.34 0 0 1-4.659 0 2.34 2.34 0 0 0-3.32-1.915 2.34 2.34 0 0 1-2.33-4.033 2.34 2.34 0 0 0 0-3.831A2.34 2.34 0 0 1 6.35 6.051a2.34 2.34 0 0 0 3.319-1.915\"></path> <circle cx=\"12\" cy=\"12\" r=\"3\"></circle>",
  "shield-check": "<path d=\"M20 13c0 5-3.5 7.5-7.66 8.95a1 1 0 0 1-.67-.01C7.5 20.5 4 18 4 13V6a1 1 0 0 1 1-1c2 0 4.5-1.2 6.24-2.72a1.17 1.17 0 0 1 1.52 0C14.51 3.81 17 5 19 5a1 1 0 0 1 1 1z\"></path> <path d=\"m9 12 2 2 4-4\"></path>",
  "sparkles": "<path d=\"M11.017 2.814a1 1 0 0 1 1.966 0l1.051 5.558a2 2 0 0 0 1.594 1.594l5.558 1.051a1 1 0 0 1 0 1.966l-5.558 1.051a2 2 0 0 0-1.594 1.594l-1.051 5.558a1 1 0 0 1-1.966 0l-1.051-5.558a2 2 0 0 0-1.594-1.594l-5.558-1.051a1 1 0 0 1 0-1.966l5.558-1.051a2 2 0 0 0 1.594-1.594z\"></path> <path d=\"M20 2v4\"></path> <path d=\"M22 4h-4\"></path> <circle cx=\"4\" cy=\"20\" r=\"2\"></circle>",
  "trash-2": "<path d=\"M10 11v6\"></path> <path d=\"M14 11v6\"></path> <path d=\"M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6\"></path> <path d=\"M3 6h18\"></path> <path d=\"M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2\"></path>",
  "trending-up": "<path d=\"M16 7h6v6\"></path> <path d=\"m22 7-8.5 8.5-5-5L2 17\"></path>",
  "upload": "<path d=\"M12 3v12\"></path> <path d=\"m17 8-5-5-5 5\"></path> <path d=\"M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4\"></path>",
  "user": "<path d=\"M19 21v-2a4 4 0 0 0-4-4H9a4 4 0 0 0-4 4v2\"></path> <circle cx=\"12\" cy=\"7\" r=\"4\"></circle>",
  "wrench": "<path d=\"M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.106-3.105c.32-.322.863-.22.983.218a6 6 0 0 1-8.259 7.057l-7.91 7.91a1 1 0 0 1-2.999-3l7.91-7.91a6 6 0 0 1 7.057-8.259c.438.12.54.662.219.984z\"></path>",
  "x": "<path d=\"M18 6 6 18\"></path> <path d=\"m6 6 12 12\"></path>"
};
const ICON_NAMES = ["arrow-right", "badge-check", "battery", "bell", "bell-ring", "calendar", "camera", "car", "car-front", "check", "chevron-left", "chevron-right", "chevrons-up-down", "circle-alert", "circle-check", "clock", "credit-card", "disc", "droplet", "eye", "eye-off", "file-text", "folder", "fuel", "funnel", "gauge", "house", "lock", "mail", "menu", "paperclip", "pencil", "plus", "receipt", "scan-line", "search", "settings", "shield-check", "sparkles", "trash-2", "trending-up", "upload", "user", "wrench", "x"];
function Icon({
  name,
  size = 20,
  color = 'currentColor',
  strokeWidth = 1.75,
  style,
  ...rest
}) {
  const d = ICONS[name];
  if (!d) return null;
  return /*#__PURE__*/React.createElement("svg", _extends({
    width: size,
    height: size,
    viewBox: "0 0 24 24",
    fill: "none",
    stroke: color,
    strokeWidth: strokeWidth,
    strokeLinecap: "round",
    strokeLinejoin: "round",
    "aria-hidden": "true",
    focusable: "false",
    style: {
      display: 'block',
      flex: 'none',
      ...style
    },
    dangerouslySetInnerHTML: {
      __html: d
    }
  }, rest));
}
Object.assign(__ds_scope, { ICONS, ICON_NAMES, Icon });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/Icon.jsx", error: String((e && e.message) || e) }); }

// components/app/DocumentChip.jsx
try { (() => {
function DocumentChip({
  name,
  size,
  kind = 'pdf',
  onRemove,
  style
}) {
  return /*#__PURE__*/React.createElement("span", {
    style: {
      display: 'inline-flex',
      alignItems: 'center',
      gap: 10,
      padding: '8px 10px',
      background: 'var(--surface-raised)',
      border: '1px solid var(--border-subtle)',
      borderRadius: 'var(--radius-sm)',
      maxWidth: '100%',
      ...style
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      width: 30,
      height: 30,
      flex: 'none',
      borderRadius: 'var(--radius-xs)',
      background: 'var(--surface-hover)',
      color: 'var(--text-muted)',
      display: 'inline-flex',
      alignItems: 'center',
      justifyContent: 'center'
    }
  }, /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: kind === 'image' ? 'camera' : 'file-text',
    size: 15
  })), /*#__PURE__*/React.createElement("span", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      minWidth: 0
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 'var(--text-body-sm)',
      color: 'var(--text-strong)',
      overflow: 'hidden',
      textOverflow: 'ellipsis',
      whiteSpace: 'nowrap'
    }
  }, name), size ? /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 'var(--text-micro)',
      color: 'var(--text-faint)',
      fontFamily: 'var(--font-mono)'
    }
  }, size) : null), onRemove ? /*#__PURE__*/React.createElement("button", {
    type: "button",
    onClick: onRemove,
    "aria-label": "Remove document",
    style: {
      background: 'transparent',
      border: 'none',
      color: 'var(--text-faint)',
      cursor: 'pointer',
      display: 'inline-flex',
      padding: 4
    }
  }, /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: "x",
    size: 15
  })) : null);
}
Object.assign(__ds_scope, { DocumentChip });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/app/DocumentChip.jsx", error: String((e && e.message) || e) }); }

// components/app/EmptyState.jsx
try { (() => {
function EmptyState({
  icon = 'folder',
  title,
  body,
  action,
  style
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      textAlign: 'center',
      gap: 10,
      padding: '32px 20px',
      ...style
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      width: 56,
      height: 56,
      borderRadius: 'var(--radius-lg)',
      background: 'var(--surface-raised)',
      border: '1px solid var(--border-subtle)',
      color: 'var(--text-faint)',
      display: 'inline-flex',
      alignItems: 'center',
      justifyContent: 'center',
      marginBottom: 4
    }
  }, /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: icon,
    size: 24
  })), /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--font-display)',
      fontSize: 'var(--text-h3)',
      fontWeight: 'var(--weight-semibold)',
      color: 'var(--text-strong)'
    }
  }, title), body ? /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 'var(--text-body-sm)',
      color: 'var(--text-muted)',
      maxWidth: 280,
      textWrap: 'pretty'
    }
  }, body) : null, action ? /*#__PURE__*/React.createElement("span", {
    style: {
      marginTop: 8
    }
  }, action) : null);
}
Object.assign(__ds_scope, { EmptyState });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/app/EmptyState.jsx", error: String((e && e.message) || e) }); }

// components/app/StatTile.jsx
try { (() => {
function StatTile({
  icon,
  label,
  value,
  unit,
  tone = 'neutral',
  onClick,
  style
}) {
  const colors = {
    neutral: 'var(--text-muted)',
    accent: 'var(--accent)',
    ok: 'var(--status-ok)',
    due: 'var(--status-due)',
    overdue: 'var(--status-overdue)'
  };
  const c = colors[tone] || colors.neutral;
  return /*#__PURE__*/React.createElement("div", {
    onClick: onClick,
    style: {
      flex: 1,
      minWidth: 0,
      padding: 14,
      borderRadius: 'var(--radius-md)',
      background: 'var(--surface-card)',
      border: '1px solid var(--border-subtle)',
      display: 'flex',
      flexDirection: 'column',
      gap: 10,
      cursor: onClick ? 'pointer' : undefined,
      ...style
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 7,
      color: c
    }
  }, /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: icon,
    size: 16
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 'var(--text-micro)',
      fontWeight: 'var(--weight-semibold)',
      letterSpacing: 'var(--track-caps)',
      textTransform: 'uppercase',
      color: 'var(--text-faint)'
    }
  }, label)), /*#__PURE__*/React.createElement("span", {
    style: {
      display: 'flex',
      alignItems: 'baseline',
      gap: 4
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--font-display)',
      fontSize: 'var(--text-h2)',
      fontWeight: 'var(--weight-bold)',
      color: tone === 'neutral' ? 'var(--text-strong)' : c,
      letterSpacing: 'var(--track-heading)'
    }
  }, value), unit ? /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 'var(--text-body-sm)',
      color: 'var(--text-faint)'
    }
  }, unit) : null));
}
Object.assign(__ds_scope, { StatTile });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/app/StatTile.jsx", error: String((e && e.message) || e) }); }

// components/app/TabBar.jsx
try { (() => {
function TabBar({
  items = [],
  value,
  onChange,
  style
}) {
  return /*#__PURE__*/React.createElement("nav", {
    style: {
      display: 'flex',
      alignItems: 'stretch',
      gap: 4,
      padding: '8px 10px 10px',
      background: 'rgba(13,15,19,0.86)',
      backdropFilter: 'var(--blur-glass)',
      WebkitBackdropFilter: 'var(--blur-glass)',
      borderTop: '1px solid var(--border-subtle)',
      ...style
    }
  }, items.map(it => {
    const active = it.value === value;
    return /*#__PURE__*/React.createElement("button", {
      key: it.value,
      type: "button",
      onClick: () => onChange && onChange(it.value),
      style: {
        flex: 1,
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        gap: 4,
        padding: '8px 0',
        background: 'transparent',
        border: 'none',
        cursor: 'pointer',
        color: active ? 'var(--accent-text)' : 'var(--text-faint)',
        transition: 'color var(--dur-fast) var(--ease-out)'
      }
    }, /*#__PURE__*/React.createElement(__ds_scope.Icon, {
      name: it.icon,
      size: 22,
      strokeWidth: active ? 2.1 : 1.75
    }), /*#__PURE__*/React.createElement("span", {
      style: {
        fontSize: 'var(--text-micro)',
        fontWeight: active ? 'var(--weight-semibold)' : 'var(--weight-medium)'
      }
    }, it.label));
  }));
}
Object.assign(__ds_scope, { TabBar });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/app/TabBar.jsx", error: String((e && e.message) || e) }); }

// components/core/Badge.jsx
try { (() => {
const TONES = {
  neutral: {
    bg: 'var(--surface-hover)',
    color: 'var(--text-muted)'
  },
  accent: {
    bg: 'var(--accent-tint)',
    color: 'var(--accent-text)'
  },
  ok: {
    bg: 'var(--status-ok-tint)',
    color: 'var(--status-ok)'
  },
  due: {
    bg: 'var(--status-due-tint)',
    color: 'var(--status-due)'
  },
  overdue: {
    bg: 'var(--status-overdue-tint)',
    color: 'var(--status-overdue)'
  },
  info: {
    bg: 'var(--status-info-tint)',
    color: 'var(--status-info)'
  }
};
function Badge({
  children,
  tone = 'neutral',
  icon,
  dot = false,
  style
}) {
  const t = TONES[tone] || TONES.neutral;
  return /*#__PURE__*/React.createElement("span", {
    style: {
      display: 'inline-flex',
      alignItems: 'center',
      gap: 6,
      height: 24,
      padding: '0 10px',
      borderRadius: 'var(--radius-pill)',
      background: t.bg,
      color: t.color,
      fontSize: 'var(--text-caption)',
      fontWeight: 'var(--weight-semibold)',
      letterSpacing: 'var(--track-body)',
      whiteSpace: 'nowrap',
      ...style
    }
  }, dot ? /*#__PURE__*/React.createElement("span", {
    style: {
      width: 6,
      height: 6,
      borderRadius: '50%',
      background: 'currentColor'
    }
  }) : null, icon ? /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: icon,
    size: 13,
    strokeWidth: 2.2
  }) : null, children);
}
Object.assign(__ds_scope, { Badge });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/Badge.jsx", error: String((e && e.message) || e) }); }

// components/app/PlanCard.jsx
try { (() => {
function PlanCard({
  name,
  price,
  period,
  tagline,
  features = [],
  selected = false,
  badge,
  onClick,
  style
}) {
  return /*#__PURE__*/React.createElement("div", {
    onClick: onClick,
    style: {
      position: 'relative',
      overflow: 'hidden',
      padding: 18,
      borderRadius: 'var(--radius-lg)',
      background: selected ? 'var(--surface-raised)' : 'var(--surface-card)',
      border: '1px solid ' + (selected ? 'var(--accent)' : 'var(--border-subtle)'),
      boxShadow: selected ? 'var(--shadow-accent)' : 'none',
      cursor: 'pointer',
      transition: 'border-color var(--dur-fast) var(--ease-out), background var(--dur-fast) var(--ease-out)',
      ...style
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between',
      gap: 10
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 8
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--font-display)',
      fontSize: 'var(--text-h3)',
      fontWeight: 'var(--weight-semibold)',
      color: 'var(--text-strong)'
    }
  }, name), badge ? /*#__PURE__*/React.createElement(__ds_scope.Badge, {
    tone: "accent"
  }, badge) : null), /*#__PURE__*/React.createElement("span", {
    style: {
      width: 22,
      height: 22,
      borderRadius: '50%',
      flex: 'none',
      border: '1px solid ' + (selected ? 'var(--accent)' : 'var(--border-strong)'),
      background: selected ? 'var(--accent)' : 'transparent',
      color: 'var(--text-on-accent)',
      display: 'inline-flex',
      alignItems: 'center',
      justifyContent: 'center'
    }
  }, selected ? /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: "check",
    size: 14,
    strokeWidth: 3
  }) : null)), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'baseline',
      gap: 5,
      marginTop: 10
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--font-display)',
      fontSize: 'var(--text-h1)',
      fontWeight: 'var(--weight-bold)',
      color: 'var(--text-strong)',
      letterSpacing: 'var(--track-display)'
    }
  }, price), period ? /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 'var(--text-body-sm)',
      color: 'var(--text-faint)'
    }
  }, period) : null), tagline ? /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 4,
      fontSize: 'var(--text-body-sm)',
      color: 'var(--text-muted)'
    }
  }, tagline) : null, features.length ? /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 8,
      marginTop: 14,
      paddingTop: 14,
      borderTop: '1px solid var(--border-subtle)'
    }
  }, features.map(ft => /*#__PURE__*/React.createElement("span", {
    key: ft,
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 9,
      fontSize: 'var(--text-body-sm)',
      color: 'var(--text-body)'
    }
  }, /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: "check",
    size: 15,
    color: "var(--status-ok)",
    strokeWidth: 2.4
  }), ft))) : null);
}
Object.assign(__ds_scope, { PlanCard });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/app/PlanCard.jsx", error: String((e && e.message) || e) }); }

// components/app/VehicleCard.jsx
try { (() => {
function VehicleCard({
  make,
  model,
  year,
  plate,
  mileage,
  status = 'ok',
  statusLabel,
  selected = false,
  onClick,
  style
}) {
  return /*#__PURE__*/React.createElement("div", {
    onClick: onClick,
    style: {
      position: 'relative',
      overflow: 'hidden',
      padding: 18,
      borderRadius: 'var(--radius-lg)',
      background: selected ? 'var(--surface-raised)' : 'var(--surface-card)',
      border: '1px solid ' + (selected ? 'rgba(91,140,255,0.45)' : 'var(--border-subtle)'),
      boxShadow: selected ? 'var(--shadow-raised)' : 'var(--shadow-card)',
      cursor: onClick ? 'pointer' : undefined,
      ...style
    }
  }, selected ? /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0,
      background: 'var(--grad-hero)',
      pointerEvents: 'none'
    }
  }) : null, /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'relative',
      display: 'flex',
      alignItems: 'flex-start',
      gap: 14
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      width: 48,
      height: 48,
      flex: 'none',
      borderRadius: 'var(--radius-md)',
      background: 'var(--accent-tint)',
      color: 'var(--accent)',
      display: 'inline-flex',
      alignItems: 'center',
      justifyContent: 'center'
    }
  }, /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: "car-front",
    size: 26
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      minWidth: 0
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: 'var(--font-display)',
      fontSize: 'var(--text-h3)',
      fontWeight: 'var(--weight-semibold)',
      color: 'var(--text-strong)',
      letterSpacing: 'var(--track-heading)'
    }
  }, make, " ", model), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 8,
      marginTop: 4,
      fontSize: 'var(--text-body-sm)',
      color: 'var(--text-muted)'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--font-mono)'
    }
  }, year), plate ? /*#__PURE__*/React.createElement("span", {
    style: {
      width: 3,
      height: 3,
      borderRadius: '50%',
      background: 'var(--text-faint)'
    }
  }) : null, plate ? /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--font-mono)',
      letterSpacing: '0.06em'
    }
  }, plate) : null)), statusLabel ? /*#__PURE__*/React.createElement(__ds_scope.Badge, {
    tone: status,
    dot: true
  }, statusLabel) : null), mileage != null ? /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'relative',
      display: 'flex',
      alignItems: 'center',
      gap: 8,
      marginTop: 16,
      paddingTop: 14,
      borderTop: '1px solid var(--border-subtle)',
      color: 'var(--text-muted)',
      fontSize: 'var(--text-body-sm)'
    }
  }, /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: "gauge",
    size: 16,
    color: "var(--text-faint)"
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--font-mono)',
      color: 'var(--text-body)'
    }
  }, mileage), /*#__PURE__*/React.createElement("span", null, "km on the clock")) : null);
}
Object.assign(__ds_scope, { VehicleCard });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/app/VehicleCard.jsx", error: String((e && e.message) || e) }); }

// components/core/Button.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
const {
  useState
} = React;
const SIZES = {
  sm: {
    height: 36,
    padding: '0 14px',
    font: 'var(--text-body-sm)',
    gap: 6,
    icon: 16,
    radius: 'var(--radius-sm)'
  },
  md: {
    height: 44,
    padding: '0 18px',
    font: 'var(--text-body-md)',
    gap: 8,
    icon: 18,
    radius: 'var(--radius-md)'
  },
  lg: {
    height: 52,
    padding: '0 22px',
    font: 'var(--text-body-lg)',
    gap: 10,
    icon: 20,
    radius: 'var(--radius-md)'
  }
};
const TONES = {
  primary: {
    bg: 'var(--accent)',
    hover: 'var(--accent-hover)',
    color: 'var(--text-on-accent)',
    border: 'transparent',
    shadow: 'var(--shadow-accent)'
  },
  secondary: {
    bg: 'var(--surface-raised)',
    hover: 'var(--surface-hover)',
    color: 'var(--text-strong)',
    border: 'var(--border-subtle)',
    shadow: 'none'
  },
  ghost: {
    bg: 'transparent',
    hover: 'var(--surface-raised)',
    color: 'var(--text-body)',
    border: 'transparent',
    shadow: 'none'
  },
  danger: {
    bg: 'var(--status-overdue-tint)',
    hover: 'rgba(255,95,95,0.22)',
    color: 'var(--status-overdue)',
    border: 'transparent',
    shadow: 'none'
  }
};
function Button({
  children,
  variant = 'primary',
  size = 'md',
  icon,
  iconEnd,
  fullWidth = false,
  disabled = false,
  pill = false,
  style,
  ...rest
}) {
  const [hover, setHover] = useState(false);
  const [press, setPress] = useState(false);
  const s = SIZES[size] || SIZES.md;
  const t = TONES[variant] || TONES.primary;
  return /*#__PURE__*/React.createElement("button", _extends({
    type: "button",
    disabled: disabled,
    onMouseEnter: () => setHover(true),
    onMouseLeave: () => {
      setHover(false);
      setPress(false);
    },
    onMouseDown: () => setPress(true),
    onMouseUp: () => setPress(false),
    style: {
      display: 'inline-flex',
      alignItems: 'center',
      justifyContent: 'center',
      gap: s.gap,
      height: s.height,
      padding: s.padding,
      width: fullWidth ? '100%' : undefined,
      fontFamily: 'var(--font-body)',
      fontSize: s.font,
      fontWeight: 'var(--weight-semibold)',
      letterSpacing: 'var(--track-body)',
      color: t.color,
      background: disabled ? 'var(--surface-raised)' : hover ? t.hover : t.bg,
      border: '1px solid ' + (disabled ? 'var(--border-subtle)' : t.border),
      borderRadius: pill ? 'var(--radius-pill)' : s.radius,
      boxShadow: variant === 'primary' && !disabled ? t.shadow : 'none',
      opacity: disabled ? 0.45 : 1,
      transform: press && !disabled ? 'scale(0.975)' : 'scale(1)',
      cursor: disabled ? 'not-allowed' : 'pointer',
      transition: 'background var(--dur-fast) var(--ease-out), transform var(--dur-fast) var(--ease-out), opacity var(--dur-fast) var(--ease-out)',
      ...style
    }
  }, rest), icon ? /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: icon,
    size: s.icon
  }) : null, children, iconEnd ? /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: iconEnd,
    size: s.icon
  }) : null);
}
Object.assign(__ds_scope, { Button });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/Button.jsx", error: String((e && e.message) || e) }); }

// components/core/Checkbox.jsx
try { (() => {
function Checkbox({
  checked = false,
  onChange,
  label,
  description,
  disabled = false,
  style
}) {
  return /*#__PURE__*/React.createElement("label", {
    style: {
      display: 'flex',
      gap: 12,
      alignItems: description ? 'flex-start' : 'center',
      cursor: disabled ? 'not-allowed' : 'pointer',
      opacity: disabled ? 0.5 : 1,
      ...style
    }
  }, /*#__PURE__*/React.createElement("input", {
    type: "checkbox",
    checked: checked,
    onChange: onChange,
    disabled: disabled,
    style: {
      position: 'absolute',
      opacity: 0,
      width: 0,
      height: 0
    }
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      width: 22,
      height: 22,
      flex: 'none',
      display: 'inline-flex',
      alignItems: 'center',
      justifyContent: 'center',
      borderRadius: 'var(--radius-xs)',
      background: checked ? 'var(--accent)' : 'var(--surface-input)',
      border: '1px solid ' + (checked ? 'var(--accent)' : 'var(--border-strong)'),
      color: 'var(--text-on-accent)',
      marginTop: description ? 1 : 0,
      transition: 'background var(--dur-fast) var(--ease-out), border-color var(--dur-fast) var(--ease-out)'
    }
  }, checked ? /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: "check",
    size: 14,
    strokeWidth: 3
  }) : null), /*#__PURE__*/React.createElement("span", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 2
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 'var(--text-body-md)',
      color: 'var(--text-strong)'
    }
  }, label), description ? /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 'var(--text-body-sm)',
      color: 'var(--text-muted)'
    }
  }, description) : null));
}
Object.assign(__ds_scope, { Checkbox });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/Checkbox.jsx", error: String((e && e.message) || e) }); }

// components/core/IconButton.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
const {
  useState
} = React;
const SIZES = {
  sm: 32,
  md: 40,
  lg: 44
};
function IconButton({
  icon,
  size = 'md',
  variant = 'ghost',
  label,
  active = false,
  style,
  ...rest
}) {
  const [hover, setHover] = useState(false);
  const px = SIZES[size] || SIZES.md;
  const bg = variant === 'solid' ? 'var(--surface-raised)' : 'transparent';
  return /*#__PURE__*/React.createElement("button", _extends({
    type: "button",
    "aria-label": label,
    onMouseEnter: () => setHover(true),
    onMouseLeave: () => setHover(false),
    style: {
      width: px,
      height: px,
      display: 'inline-flex',
      alignItems: 'center',
      justifyContent: 'center',
      background: active ? 'var(--accent-tint)' : hover ? 'var(--surface-hover)' : bg,
      color: active ? 'var(--accent-text)' : 'var(--text-body)',
      border: variant === 'solid' ? '1px solid var(--border-subtle)' : '1px solid transparent',
      borderRadius: 'var(--radius-sm)',
      cursor: 'pointer',
      transition: 'background var(--dur-fast) var(--ease-out), color var(--dur-fast) var(--ease-out)',
      ...style
    }
  }, rest), /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: icon,
    size: size === 'sm' ? 16 : 20
  }));
}
Object.assign(__ds_scope, { IconButton });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/IconButton.jsx", error: String((e && e.message) || e) }); }

// components/app/AppHeader.jsx
try { (() => {
function AppHeader({
  title,
  eyebrow,
  back = false,
  onBack,
  actions,
  style
}) {
  return /*#__PURE__*/React.createElement("header", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 10,
      minHeight: 56,
      padding: '8px var(--gutter-screen)',
      background: 'var(--surface-app)',
      ...style
    }
  }, back ? /*#__PURE__*/React.createElement(__ds_scope.IconButton, {
    icon: "chevron-left",
    label: "Back",
    onClick: onBack,
    style: {
      marginLeft: -8
    }
  }) : null, /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      minWidth: 0
    }
  }, eyebrow ? /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 'var(--text-micro)',
      fontWeight: 'var(--weight-semibold)',
      letterSpacing: 'var(--track-caps)',
      textTransform: 'uppercase',
      color: 'var(--text-faint)'
    }
  }, eyebrow) : null, /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: 'var(--font-display)',
      fontSize: 'var(--text-h2)',
      fontWeight: 'var(--weight-semibold)',
      color: 'var(--text-strong)',
      letterSpacing: 'var(--track-heading)',
      overflow: 'hidden',
      textOverflow: 'ellipsis',
      whiteSpace: 'nowrap'
    }
  }, title)), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 4
    }
  }, actions));
}
Object.assign(__ds_scope, { AppHeader });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/app/AppHeader.jsx", error: String((e && e.message) || e) }); }

// components/core/Input.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
const {
  useState
} = React;
function Input({
  label,
  hint,
  error,
  icon,
  suffix,
  value,
  onChange,
  placeholder,
  type = 'text',
  mono = false,
  disabled = false,
  style,
  ...rest
}) {
  const [focus, setFocus] = useState(false);
  const borderColor = error ? 'var(--status-overdue)' : focus ? 'var(--border-focus)' : 'var(--border-subtle)';
  return /*#__PURE__*/React.createElement("label", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 6,
      width: '100%',
      ...style
    }
  }, label ? /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 'var(--text-body-sm)',
      fontWeight: 'var(--weight-medium)',
      color: 'var(--text-muted)'
    }
  }, label) : null, /*#__PURE__*/React.createElement("span", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 10,
      height: 48,
      padding: '0 14px',
      background: 'var(--surface-input)',
      border: '1px solid ' + borderColor,
      borderRadius: 'var(--radius-md)',
      opacity: disabled ? 0.5 : 1,
      boxShadow: focus ? '0 0 0 3px var(--accent-tint)' : 'none',
      transition: 'border-color var(--dur-fast) var(--ease-out), box-shadow var(--dur-fast) var(--ease-out)'
    }
  }, icon ? /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: icon,
    size: 18,
    color: "var(--text-faint)"
  }) : null, /*#__PURE__*/React.createElement("input", _extends({
    type: type,
    value: value,
    onChange: onChange,
    placeholder: placeholder,
    disabled: disabled,
    onFocus: () => setFocus(true),
    onBlur: () => setFocus(false),
    style: {
      flex: 1,
      minWidth: 0,
      background: 'transparent',
      border: 'none',
      outline: 'none',
      color: 'var(--text-strong)',
      fontFamily: mono ? 'var(--font-mono)' : 'var(--font-body)',
      fontSize: 'var(--text-body-lg)',
      letterSpacing: mono ? '0.02em' : 'var(--track-body)'
    }
  }, rest)), suffix ? /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 'var(--text-body-sm)',
      color: 'var(--text-faint)',
      fontFamily: 'var(--font-mono)'
    }
  }, suffix) : null), error ? /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 'var(--text-caption)',
      color: 'var(--status-overdue)'
    }
  }, error) : hint ? /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 'var(--text-caption)',
      color: 'var(--text-faint)'
    }
  }, hint) : null);
}
Object.assign(__ds_scope, { Input });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/Input.jsx", error: String((e && e.message) || e) }); }

// components/core/ListRow.jsx
try { (() => {
const {
  useState
} = React;
function ListRow({
  icon,
  iconColor = 'var(--text-muted)',
  iconBg,
  title,
  subtitle,
  meta,
  metaSub,
  trailing,
  chevron = false,
  onClick,
  style
}) {
  const [hover, setHover] = useState(false);
  return /*#__PURE__*/React.createElement("div", {
    onClick: onClick,
    onMouseEnter: () => setHover(true),
    onMouseLeave: () => setHover(false),
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 12,
      minHeight: 'var(--tap-min)',
      padding: '12px 4px',
      background: hover && onClick ? 'var(--surface-hover)' : 'transparent',
      borderRadius: 'var(--radius-sm)',
      cursor: onClick ? 'pointer' : undefined,
      transition: 'background var(--dur-fast) var(--ease-out)',
      ...style
    }
  }, icon ? /*#__PURE__*/React.createElement("span", {
    style: {
      width: 38,
      height: 38,
      flex: 'none',
      borderRadius: 'var(--radius-sm)',
      background: iconBg || 'var(--surface-raised)',
      color: iconColor,
      display: 'inline-flex',
      alignItems: 'center',
      justifyContent: 'center'
    }
  }, /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: icon,
    size: 18
  })) : null, /*#__PURE__*/React.createElement("span", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 2,
      minWidth: 0,
      flex: 1
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 'var(--text-body-md)',
      fontWeight: 'var(--weight-semibold)',
      color: 'var(--text-strong)',
      overflow: 'hidden',
      textOverflow: 'ellipsis',
      whiteSpace: 'nowrap'
    }
  }, title), subtitle ? /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 'var(--text-body-sm)',
      color: 'var(--text-muted)'
    }
  }, subtitle) : null), meta || metaSub ? /*#__PURE__*/React.createElement("span", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'flex-end',
      gap: 2
    }
  }, meta ? /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 'var(--text-body-sm)',
      fontFamily: 'var(--font-mono)',
      color: 'var(--text-strong)'
    }
  }, meta) : null, metaSub ? /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 'var(--text-caption)',
      color: 'var(--text-faint)'
    }
  }, metaSub) : null) : null, trailing, chevron ? /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: "chevron-right",
    size: 18,
    color: "var(--text-faint)"
  }) : null);
}
Object.assign(__ds_scope, { ListRow });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/ListRow.jsx", error: String((e && e.message) || e) }); }

// components/app/MaintenanceRow.jsx
try { (() => {
const ICONS = {
  oil: 'droplet',
  tires: 'disc',
  brakes: 'circle-alert',
  inspection: 'scan-line',
  repair: 'wrench',
  battery: 'battery',
  filters: 'funnel',
  suspension: 'car',
  other: 'wrench'
};
function MaintenanceRow({
  type = 'other',
  title,
  date,
  cost,
  mileage,
  hasDocument = false,
  onClick,
  style
}) {
  return /*#__PURE__*/React.createElement(__ds_scope.ListRow, {
    icon: ICONS[type] || 'wrench',
    iconBg: "var(--surface-raised)",
    iconColor: "var(--text-body)",
    title: title,
    subtitle: hasDocument ? date + ' · receipt attached' : date,
    meta: cost,
    metaSub: mileage ? mileage + ' km' : undefined,
    onClick: onClick,
    chevron: true,
    style: style
  });
}
Object.assign(__ds_scope, { MaintenanceRow });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/app/MaintenanceRow.jsx", error: String((e && e.message) || e) }); }

// components/app/ObligationRow.jsx
try { (() => {
const ICONS = {
  insurance: 'shield-check',
  coverage: 'badge-check',
  inspection: 'scan-line',
  roadTax: 'receipt',
  tax: 'receipt',
  other: 'file-text'
};
const TINTS = {
  ok: {
    bg: 'var(--status-ok-tint)',
    fg: 'var(--status-ok)'
  },
  due: {
    bg: 'var(--status-due-tint)',
    fg: 'var(--status-due)'
  },
  overdue: {
    bg: 'var(--status-overdue-tint)',
    fg: 'var(--status-overdue)'
  }
};
function ObligationRow({
  type = 'insurance',
  title,
  provider,
  validUntil,
  status = 'ok',
  statusLabel,
  hasDocument = false,
  onClick,
  style
}) {
  const t = TINTS[status] || TINTS.ok;
  return /*#__PURE__*/React.createElement(__ds_scope.ListRow, {
    icon: ICONS[type] || 'file-text',
    iconBg: t.bg,
    iconColor: t.fg,
    title: title,
    subtitle: provider ? provider + ' · until ' + validUntil : 'Valid until ' + validUntil,
    trailing: /*#__PURE__*/React.createElement("span", {
      style: {
        display: 'flex',
        alignItems: 'center',
        gap: 8
      }
    }, hasDocument ? /*#__PURE__*/React.createElement("span", {
      style: {
        fontSize: 'var(--text-micro)',
        color: 'var(--text-faint)',
        fontFamily: 'var(--font-mono)'
      }
    }, "PDF") : null, statusLabel ? /*#__PURE__*/React.createElement(__ds_scope.Badge, {
      tone: status
    }, statusLabel) : null),
    onClick: onClick,
    chevron: true,
    style: style
  });
}
Object.assign(__ds_scope, { ObligationRow });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/app/ObligationRow.jsx", error: String((e && e.message) || e) }); }

// components/core/ProgressBar.jsx
try { (() => {
function ProgressBar({
  value = 0,
  tone = 'accent',
  height = 6,
  label,
  valueLabel,
  style
}) {
  const colors = {
    accent: 'var(--accent)',
    ok: 'var(--status-ok)',
    due: 'var(--status-due)',
    overdue: 'var(--status-overdue)'
  };
  const pct = Math.max(0, Math.min(100, value));
  return /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 6,
      width: '100%',
      ...style
    }
  }, label || valueLabel ? /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      justifyContent: 'space-between',
      fontSize: 'var(--text-caption)'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      color: 'var(--text-muted)'
    }
  }, label), /*#__PURE__*/React.createElement("span", {
    style: {
      color: 'var(--text-body)',
      fontFamily: 'var(--font-mono)'
    }
  }, valueLabel)) : null, /*#__PURE__*/React.createElement("div", {
    style: {
      height,
      borderRadius: 'var(--radius-pill)',
      background: 'var(--surface-hover)',
      overflow: 'hidden'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      width: pct + '%',
      height: '100%',
      borderRadius: 'var(--radius-pill)',
      background: colors[tone] || colors.accent,
      transition: 'width var(--dur-slow) var(--ease-out)'
    }
  })));
}
Object.assign(__ds_scope, { ProgressBar });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/ProgressBar.jsx", error: String((e && e.message) || e) }); }

// components/core/SegmentedControl.jsx
try { (() => {
function SegmentedControl({
  options = [],
  value,
  onChange,
  size = 'md',
  style
}) {
  const h = size === 'sm' ? 34 : 40;
  return /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'inline-flex',
      padding: 3,
      gap: 2,
      background: 'var(--surface-input)',
      border: '1px solid var(--border-subtle)',
      borderRadius: 'var(--radius-md)',
      ...style
    }
  }, options.map(o => {
    const active = o.value === value;
    return /*#__PURE__*/React.createElement("button", {
      key: o.value,
      type: "button",
      onClick: () => onChange && onChange(o.value),
      style: {
        height: h,
        padding: '0 16px',
        border: 'none',
        borderRadius: 'var(--radius-sm)',
        cursor: 'pointer',
        background: active ? 'var(--surface-hover)' : 'transparent',
        color: active ? 'var(--text-strong)' : 'var(--text-muted)',
        fontFamily: 'var(--font-body)',
        fontSize: 'var(--text-body-sm)',
        fontWeight: active ? 'var(--weight-semibold)' : 'var(--weight-medium)',
        transition: 'background var(--dur-fast) var(--ease-out), color var(--dur-fast) var(--ease-out)'
      }
    }, o.label);
  }));
}
Object.assign(__ds_scope, { SegmentedControl });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/SegmentedControl.jsx", error: String((e && e.message) || e) }); }

// components/core/Select.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
function Select({
  label,
  hint,
  value,
  onChange,
  options = [],
  placeholder = 'Select…',
  style,
  ...rest
}) {
  return /*#__PURE__*/React.createElement("label", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 6,
      width: '100%',
      ...style
    }
  }, label ? /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 'var(--text-body-sm)',
      fontWeight: 'var(--weight-medium)',
      color: 'var(--text-muted)'
    }
  }, label) : null, /*#__PURE__*/React.createElement("span", {
    style: {
      position: 'relative',
      display: 'flex',
      alignItems: 'center'
    }
  }, /*#__PURE__*/React.createElement("select", _extends({
    value: value,
    onChange: onChange,
    style: {
      appearance: 'none',
      width: '100%',
      height: 48,
      padding: '0 40px 0 14px',
      background: 'var(--surface-input)',
      border: '1px solid var(--border-subtle)',
      borderRadius: 'var(--radius-md)',
      color: value ? 'var(--text-strong)' : 'var(--text-faint)',
      fontFamily: 'var(--font-body)',
      fontSize: 'var(--text-body-lg)',
      outline: 'none',
      cursor: 'pointer'
    }
  }, rest), /*#__PURE__*/React.createElement("option", {
    value: "",
    disabled: true
  }, placeholder), options.map(o => /*#__PURE__*/React.createElement("option", {
    key: o.value,
    value: o.value
  }, o.label))), /*#__PURE__*/React.createElement("span", {
    style: {
      position: 'absolute',
      right: 12,
      pointerEvents: 'none',
      color: 'var(--text-faint)'
    }
  }, /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: "chevrons-up-down",
    size: 18
  }))), hint ? /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 'var(--text-caption)',
      color: 'var(--text-faint)'
    }
  }, hint) : null);
}
Object.assign(__ds_scope, { Select });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/Select.jsx", error: String((e && e.message) || e) }); }

// components/core/Switch.jsx
try { (() => {
function Switch({
  checked = false,
  onChange,
  label,
  description,
  disabled = false,
  style
}) {
  const track = /*#__PURE__*/React.createElement("span", {
    role: "switch",
    "aria-checked": checked,
    onClick: () => !disabled && onChange && onChange(!checked),
    style: {
      width: 46,
      height: 28,
      flex: 'none',
      borderRadius: 'var(--radius-pill)',
      padding: 3,
      background: checked ? 'var(--accent)' : 'var(--surface-hover)',
      border: '1px solid ' + (checked ? 'var(--accent)' : 'var(--border-strong)'),
      display: 'inline-flex',
      alignItems: 'center',
      cursor: disabled ? 'not-allowed' : 'pointer',
      transition: 'background var(--dur-base) var(--ease-out)'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      width: 20,
      height: 20,
      borderRadius: '50%',
      background: checked ? 'var(--text-on-accent)' : 'var(--text-muted)',
      transform: 'translateX(' + (checked ? 18 : 0) + 'px)',
      transition: 'transform var(--dur-base) var(--ease-spring)'
    }
  }));
  if (!label) return /*#__PURE__*/React.createElement("span", {
    style: {
      opacity: disabled ? 0.5 : 1,
      ...style
    }
  }, track);
  return /*#__PURE__*/React.createElement("span", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 14,
      opacity: disabled ? 0.5 : 1,
      ...style
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 2,
      flex: 1
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 'var(--text-body-md)',
      color: 'var(--text-strong)'
    }
  }, label), description ? /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 'var(--text-body-sm)',
      color: 'var(--text-muted)'
    }
  }, description) : null), track);
}
Object.assign(__ds_scope, { Switch });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/Switch.jsx", error: String((e && e.message) || e) }); }

// ui_kits/mobile-app/add-car-screen.jsx
try { (() => {
const {
  AppHeader,
  Input,
  Select,
  Button,
  Card,
  SectionHeader,
  DocumentChip,
  Switch,
  Icon,
  Badge
} = window.__GB;
function AddCarScreen({
  onDone,
  onBack
}) {
  const [remind, setRemind] = React.useState(true);
  const [doc, setDoc] = React.useState(true);
  return /*#__PURE__*/React.createElement(Screen, null, /*#__PURE__*/React.createElement(AppHeader, {
    eyebrow: "Garage",
    title: "Add a car",
    back: true,
    onBack: onBack
  }), /*#__PURE__*/React.createElement(Body, null, /*#__PURE__*/React.createElement(Card, {
    padding: 16,
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 14
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      width: 52,
      height: 52,
      flex: 'none',
      borderRadius: 'var(--radius-md)',
      background: 'var(--accent-tint)',
      color: 'var(--accent)',
      display: 'inline-flex',
      alignItems: 'center',
      justifyContent: 'center'
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: "car-front",
    size: 28
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 14,
      color: 'var(--text-strong)',
      fontWeight: 600
    }
  }, "Scan the registration"), /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 13,
      color: 'var(--text-muted)',
      marginTop: 2
    }
  }, "We'll fill most of this in for you.")), /*#__PURE__*/React.createElement(Button, {
    size: "sm",
    variant: "secondary",
    icon: "camera"
  }, "Scan")), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 24
    }
  }, /*#__PURE__*/React.createElement(SectionHeader, {
    title: "The basics"
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 12
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 12
    }
  }, /*#__PURE__*/React.createElement(Input, {
    label: "Make",
    placeholder: "Volkswagen",
    defaultValue: "Volkswagen"
  }), /*#__PURE__*/React.createElement(Input, {
    label: "Model",
    placeholder: "Passat",
    defaultValue: "Passat"
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 12
    }
  }, /*#__PURE__*/React.createElement(Input, {
    label: "Year",
    mono: true,
    placeholder: "2018",
    defaultValue: "2018"
  }), /*#__PURE__*/React.createElement(Input, {
    label: "Engine",
    placeholder: "2.0 TDI",
    defaultValue: "2.0 TDI"
  })), /*#__PURE__*/React.createElement(Input, {
    label: "Registration plate",
    icon: "car",
    mono: true,
    placeholder: "CA 1234 KX",
    defaultValue: "CA 1234 KX"
  }), /*#__PURE__*/React.createElement(Input, {
    label: "VIN",
    icon: "scan-line",
    mono: true,
    placeholder: "17 characters",
    hint: "Optional \u2014 handy when ordering parts."
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 24
    }
  }, /*#__PURE__*/React.createElement(SectionHeader, {
    title: "Odometer"
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 12
    }
  }, /*#__PURE__*/React.createElement(Input, {
    label: "At purchase",
    mono: true,
    suffix: "km",
    defaultValue: "152 000"
  }), /*#__PURE__*/React.createElement(Input, {
    label: "Right now",
    icon: "gauge",
    mono: true,
    suffix: "km",
    defaultValue: "184 300",
    hint: "We bump this up as you log services."
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 24
    }
  }, /*#__PURE__*/React.createElement(SectionHeader, {
    title: "First document"
  }), /*#__PURE__*/React.createElement(Card, {
    padding: 14
  }, doc ? /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 12
    }
  }, /*#__PURE__*/React.createElement(DocumentChip, {
    name: "registration-passat.pdf",
    size: "1.2 MB",
    onRemove: () => setDoc(false)
  }), /*#__PURE__*/React.createElement(Select, {
    label: "What is it?",
    options: [{
      value: 'reg',
      label: 'Registration certificate'
    }, {
      value: 'ins',
      label: 'Insurance policy'
    }, {
      value: 'inv',
      label: 'Purchase invoice'
    }],
    defaultValue: "reg"
  })) : /*#__PURE__*/React.createElement("button", {
    type: "button",
    onClick: () => setDoc(true),
    style: {
      width: '100%',
      padding: '22px 12px',
      background: 'transparent',
      border: '1px dashed var(--border-strong)',
      borderRadius: 'var(--radius-md)',
      color: 'var(--text-muted)',
      cursor: 'pointer',
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      gap: 8,
      fontFamily: 'var(--font-body)',
      fontSize: 13
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: "upload",
    size: 20
  }), "Drop the registration in \u2014 PDF or photo"))), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 24
    }
  }, /*#__PURE__*/React.createElement(SectionHeader, {
    title: "Reminders"
  }), /*#__PURE__*/React.createElement(Card, {
    padding: 16,
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 16
    }
  }, /*#__PURE__*/React.createElement(Switch, {
    checked: remind,
    onChange: setRemind,
    label: "Nudge me before renewals",
    description: "30, 7 and 1 day before each date"
  }), /*#__PURE__*/React.createElement(Switch, {
    checked: false,
    onChange: () => {},
    label: "Service interval reminders",
    description: "Every 15 000 km or 12 months"
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 24,
      display: 'flex',
      gap: 12
    }
  }, /*#__PURE__*/React.createElement(Button, {
    variant: "ghost",
    size: "lg",
    onClick: onBack
  }, "Cancel"), /*#__PURE__*/React.createElement(Button, {
    variant: "primary",
    size: "lg",
    fullWidth: true,
    icon: "check",
    onClick: onDone
  }, "Park it in the garage"))));
}
Object.assign(window, {
  AddCarScreen
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/mobile-app/add-car-screen.jsx", error: String((e && e.message) || e) }); }

// ui_kits/mobile-app/auth-screen.jsx
try { (() => {
const {
  Button,
  Input,
  SsoButton,
  Checkbox,
  SegmentedControl
} = window.__GB;
function AuthScreen({
  onAuthed
}) {
  const [mode, setMode] = React.useState('login');
  const [show, setShow] = React.useState(false);
  return /*#__PURE__*/React.createElement(Screen, null, /*#__PURE__*/React.createElement(Body, {
    style: {
      flex: 1,
      paddingTop: 20
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 10
    }
  }, /*#__PURE__*/React.createElement("img", {
    src: "../../assets/logo-dark.svg",
    width: "34",
    height: "34",
    alt: ""
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--font-display)',
      fontWeight: 700,
      fontSize: 19,
      color: 'var(--text-strong)',
      letterSpacing: '-0.02em'
    }
  }, "GarageBox")), /*#__PURE__*/React.createElement("h1", {
    style: {
      fontSize: 'var(--text-h1)',
      letterSpacing: 'var(--track-heading)',
      marginTop: 22
    }
  }, mode === 'login' ? 'Welcome back' : 'Create your garage'), /*#__PURE__*/React.createElement("p", {
    style: {
      fontSize: 15,
      color: 'var(--text-muted)',
      marginTop: 6
    }
  }, mode === 'login' ? 'Your papers are exactly where you left them.' : 'Two fields now, zero paper piles later.'), /*#__PURE__*/React.createElement(SegmentedControl, {
    style: {
      marginTop: 18,
      alignSelf: 'stretch'
    },
    options: [{
      value: 'login',
      label: 'Log in'
    }, {
      value: 'register',
      label: 'Register'
    }],
    value: mode,
    onChange: setMode
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 10,
      marginTop: 18
    }
  }, /*#__PURE__*/React.createElement(SsoButton, {
    provider: "google"
  }), /*#__PURE__*/React.createElement(SsoButton, {
    provider: "apple"
  }), /*#__PURE__*/React.createElement(SsoButton, {
    provider: "facebook"
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 12,
      margin: '18px 0'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      flex: 1,
      height: 1,
      background: 'var(--border-subtle)'
    }
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 12,
      color: 'var(--text-faint)'
    }
  }, "or with email"), /*#__PURE__*/React.createElement("span", {
    style: {
      flex: 1,
      height: 1,
      background: 'var(--border-subtle)'
    }
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 12
    }
  }, /*#__PURE__*/React.createElement(Input, {
    label: "Email",
    icon: "mail",
    placeholder: "ivan@example.com",
    defaultValue: "ivan@example.com"
  }), /*#__PURE__*/React.createElement(Input, {
    label: "Password",
    icon: "lock",
    type: show ? 'text' : 'password',
    defaultValue: "correcthorse"
  }), mode === 'register' ? /*#__PURE__*/React.createElement(Checkbox, {
    checked: true,
    label: "Email me renewal reminders",
    description: "Insurance, inspection, road tax"
  }) : null), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center',
      marginTop: 12
    }
  }, /*#__PURE__*/React.createElement("button", {
    type: "button",
    onClick: () => setShow(!show),
    style: {
      background: 'none',
      border: 'none',
      padding: 0,
      color: 'var(--text-muted)',
      fontFamily: 'var(--font-body)',
      fontSize: 13,
      cursor: 'pointer'
    }
  }, show ? 'Hide' : 'Show', " password"), mode === 'login' ? /*#__PURE__*/React.createElement("a", {
    href: "#",
    onClick: e => e.preventDefault(),
    style: {
      fontSize: 13
    }
  }, "Forgot it?") : null), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 22
    }
  }, /*#__PURE__*/React.createElement(Button, {
    variant: "primary",
    size: "lg",
    fullWidth: true,
    onClick: onAuthed
  }, mode === 'login' ? 'Log in' : 'Create account'))));
}
Object.assign(window, {
  AuthScreen
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/mobile-app/auth-screen.jsx", error: String((e && e.message) || e) }); }

// ui_kits/mobile-app/dashboard-screen.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
const {
  AppHeader,
  IconButton,
  VehicleCard,
  StatTile,
  SectionHeader,
  Card,
  ObligationRow,
  MaintenanceRow,
  TabBar,
  Button,
  Badge,
  Icon
} = window.__GB;
const CARS = [{
  make: 'Volkswagen',
  model: 'Passat',
  year: 2018,
  plate: 'CA 1234 KX',
  mileage: '184 300',
  status: 'due',
  statusLabel: '2 due soon'
}, {
  make: 'Toyota',
  model: 'Yaris',
  year: 2021,
  plate: 'CB 8842 MP',
  mileage: '61 040',
  status: 'ok',
  statusLabel: 'All good'
}];
function DashboardScreen({
  onAddCar
}) {
  const [car, setCar] = React.useState(0);
  const [tab, setTab] = React.useState('home');
  return /*#__PURE__*/React.createElement(React.Fragment, null, /*#__PURE__*/React.createElement(Screen, null, /*#__PURE__*/React.createElement(AppHeader, {
    eyebrow: "Your garage",
    title: "Hello, Ivan",
    actions: /*#__PURE__*/React.createElement(React.Fragment, null, /*#__PURE__*/React.createElement(IconButton, {
      icon: "search",
      label: "Search"
    }), /*#__PURE__*/React.createElement(IconButton, {
      icon: "bell",
      label: "Reminders"
    }))
  }), /*#__PURE__*/React.createElement(Body, null, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 10,
      overflowX: 'auto',
      paddingBottom: 4,
      margin: '0 -4px',
      padding: '0 4px 4px'
    }
  }, CARS.map((c, i) => /*#__PURE__*/React.createElement("div", {
    key: c.plate,
    style: {
      minWidth: 300,
      flex: 'none'
    }
  }, /*#__PURE__*/React.createElement(VehicleCard, _extends({}, c, {
    selected: i === car,
    onClick: () => setCar(i)
  })))), /*#__PURE__*/React.createElement("button", {
    type: "button",
    onClick: onAddCar,
    style: {
      minWidth: 96,
      flex: 'none',
      borderRadius: 'var(--radius-lg)',
      background: 'transparent',
      border: '1px dashed var(--border-strong)',
      color: 'var(--text-muted)',
      cursor: 'pointer',
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      justifyContent: 'center',
      gap: 8,
      fontFamily: 'var(--font-body)',
      fontSize: 13
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: "plus",
    size: 20
  }), "Add car")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 10,
      marginTop: 16
    }
  }, /*#__PURE__*/React.createElement(StatTile, {
    icon: "circle-alert",
    label: "Overdue",
    value: car === 0 ? 1 : 0,
    tone: car === 0 ? 'overdue' : 'ok'
  }), /*#__PURE__*/React.createElement(StatTile, {
    icon: "clock",
    label: "Due soon",
    value: car === 0 ? 2 : 1,
    tone: "due"
  }), /*#__PURE__*/React.createElement(StatTile, {
    icon: "folder",
    label: "Docs",
    value: car === 0 ? 14 : 6
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 24
    }
  }, /*#__PURE__*/React.createElement(SectionHeader, {
    title: "Renewals",
    action: "See all"
  }), /*#__PURE__*/React.createElement(Card, {
    padding: 8
  }, car === 0 ? /*#__PURE__*/React.createElement(React.Fragment, null, /*#__PURE__*/React.createElement(ObligationRow, {
    type: "inspection",
    title: "Roadworthiness inspection",
    provider: "AutoTest Center",
    validUntil: "02 May 2026",
    status: "overdue",
    statusLabel: "9 days overdue",
    hasDocument: true
  }), /*#__PURE__*/React.createElement(ObligationRow, {
    type: "roadTax",
    title: "Road tax sticker",
    provider: "Transport agency",
    validUntil: "24 Aug 2026",
    status: "due",
    statusLabel: "18 days left"
  }), /*#__PURE__*/React.createElement(ObligationRow, {
    type: "insurance",
    title: "Liability insurance",
    provider: "Northbridge",
    validUntil: "14 Sep 2026",
    status: "ok",
    statusLabel: "Valid",
    hasDocument: true
  })) : /*#__PURE__*/React.createElement(React.Fragment, null, /*#__PURE__*/React.createElement(ObligationRow, {
    type: "insurance",
    title: "Liability insurance",
    provider: "Northbridge",
    validUntil: "30 Aug 2026",
    status: "due",
    statusLabel: "24 days left",
    hasDocument: true
  }), /*#__PURE__*/React.createElement(ObligationRow, {
    type: "coverage",
    title: "Full coverage",
    provider: "Northbridge",
    validUntil: "30 Jan 2027",
    status: "ok",
    statusLabel: "Valid",
    hasDocument: true
  })))), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 24
    }
  }, /*#__PURE__*/React.createElement(SectionHeader, {
    title: "Recent services",
    action: "See all"
  }), /*#__PURE__*/React.createElement(Card, {
    padding: 8
  }, /*#__PURE__*/React.createElement(MaintenanceRow, {
    type: "oil",
    title: "Oil & filter change",
    date: "12 Mar 2026",
    cost: "\u20AC89",
    mileage: "181 200",
    hasDocument: true
  }), /*#__PURE__*/React.createElement(MaintenanceRow, {
    type: "brakes",
    title: "Front brake pads",
    date: "28 Jan 2026",
    cost: "\u20AC164",
    mileage: "178 450"
  }), /*#__PURE__*/React.createElement(MaintenanceRow, {
    type: "tires",
    title: "Winter tires on",
    date: "04 Nov 2025",
    cost: "\u20AC40",
    mileage: "176 900",
    hasDocument: true
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 20
    }
  }, /*#__PURE__*/React.createElement(Card, {
    tone: "accent",
    padding: 16
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 12,
      alignItems: 'flex-start'
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: "sparkles",
    size: 20,
    color: "var(--accent)"
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: 'var(--font-display)',
      fontSize: 15,
      fontWeight: 600,
      color: 'var(--text-strong)'
    }
  }, "3 of 5 free documents used"), /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 13,
      color: 'var(--text-muted)',
      marginTop: 4
    }
  }, "Go unlimited for \u20AC5 a month.")), /*#__PURE__*/React.createElement(Button, {
    size: "sm"
  }, "Upgrade")))))), /*#__PURE__*/React.createElement(TabBar, {
    items: [{
      value: 'home',
      label: 'Garage',
      icon: 'house'
    }, {
      value: 'records',
      label: 'Records',
      icon: 'wrench'
    }, {
      value: 'add',
      label: 'Add',
      icon: 'plus'
    }, {
      value: 'docs',
      label: 'Documents',
      icon: 'folder'
    }, {
      value: 'me',
      label: 'Profile',
      icon: 'user'
    }],
    value: tab,
    onChange: v => v === 'add' ? onAddCar() : setTab(v)
  }));
}
Object.assign(window, {
  DashboardScreen
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/mobile-app/dashboard-screen.jsx", error: String((e && e.message) || e) }); }

// ui_kits/mobile-app/onboarding-screen.jsx
try { (() => {
const {
  Button,
  Card,
  Badge,
  VehicleCard,
  ObligationRow,
  MaintenanceRow,
  Icon
} = window.__GB;
function PreviewStack() {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'relative',
      height: 268,
      marginTop: 4
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: '-40px -60px auto',
      height: 300,
      background: 'var(--grad-hero)',
      pointerEvents: 'none'
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: 6,
      top: 26,
      right: 44,
      transform: 'rotate(-4deg)',
      opacity: 0.55
    }
  }, /*#__PURE__*/React.createElement(Card, {
    padding: 10
  }, /*#__PURE__*/React.createElement(MaintenanceRow, {
    type: "tires",
    title: "Winter tires on",
    date: "04 Nov 2025",
    cost: "\u20AC40",
    mileage: "176 900"
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: 44,
      top: 96,
      right: 4,
      transform: 'rotate(3deg)'
    }
  }, /*#__PURE__*/React.createElement(Card, {
    padding: 10
  }, /*#__PURE__*/React.createElement(ObligationRow, {
    type: "insurance",
    title: "Liability insurance",
    validUntil: "14 Sep 2026",
    status: "ok",
    statusLabel: "Valid",
    hasDocument: true
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: 12,
      top: 170,
      right: 24
    }
  }, /*#__PURE__*/React.createElement(VehicleCard, {
    make: "Volkswagen",
    model: "Passat",
    year: 2018,
    plate: "CA 1234 KX",
    mileage: "184 300",
    status: "due",
    statusLabel: "2 due soon",
    selected: true
  })));
}
function OnboardingScreen({
  onNext,
  onSkip
}) {
  return /*#__PURE__*/React.createElement(Screen, null, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between',
      padding: '4px var(--gutter-screen) 0'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 9
    }
  }, /*#__PURE__*/React.createElement("img", {
    src: "../../assets/logo-dark.svg",
    width: "30",
    height: "30",
    alt: ""
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--font-display)',
      fontWeight: 700,
      fontSize: 17,
      color: 'var(--text-strong)',
      letterSpacing: '-0.02em'
    }
  }, "GarageBox")), /*#__PURE__*/React.createElement("button", {
    type: "button",
    onClick: onSkip,
    style: {
      background: 'none',
      border: 'none',
      color: 'var(--text-muted)',
      fontFamily: 'var(--font-body)',
      fontSize: 14,
      cursor: 'pointer'
    }
  }, "Skip")), /*#__PURE__*/React.createElement(Body, {
    style: {
      flex: 1,
      justifyContent: 'space-between',
      paddingTop: 8
    }
  }, /*#__PURE__*/React.createElement(PreviewStack, null), /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement(Badge, {
    tone: "accent",
    icon: "sparkles"
  }, "Glovebox, drawer, inbox \u2014 sorted"), /*#__PURE__*/React.createElement("h1", {
    style: {
      fontSize: 'var(--text-display)',
      lineHeight: 'var(--text-display-lh)',
      letterSpacing: 'var(--track-display)',
      marginTop: 14,
      textWrap: 'pretty'
    }
  }, "Your car's paperwork lives in six places."), /*#__PURE__*/React.createElement("p", {
    style: {
      fontSize: 16,
      color: 'var(--text-muted)',
      marginTop: 12,
      textWrap: 'pretty'
    }
  }, "GarageBox keeps every service, renewal and PDF against the right car \u2014 and nudges you before anything expires."), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 6,
      margin: '22px 0 16px'
    }
  }, [0, 1, 2].map(i => /*#__PURE__*/React.createElement("span", {
    key: i,
    style: {
      height: 4,
      flex: i === 0 ? 2 : 1,
      borderRadius: 999,
      background: i === 0 ? 'var(--accent)' : 'var(--surface-hover)'
    }
  }))), /*#__PURE__*/React.createElement(Button, {
    variant: "primary",
    size: "lg",
    fullWidth: true,
    iconEnd: "arrow-right",
    onClick: onNext
  }, "Get started"))));
}
Object.assign(window, {
  OnboardingScreen
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/mobile-app/onboarding-screen.jsx", error: String((e && e.message) || e) }); }

// ui_kits/mobile-app/phone-shell.jsx
try { (() => {
const {
  useState
} = React;
function Phone({
  children,
  label
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      gap: 10
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      width: 390,
      height: 844,
      borderRadius: 46,
      background: 'var(--surface-app)',
      border: '1px solid var(--border-strong)',
      boxShadow: 'var(--shadow-overlay)',
      overflow: 'hidden',
      display: 'flex',
      flexDirection: 'column',
      position: 'relative'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      height: 52,
      flex: 'none',
      display: 'flex',
      alignItems: 'flex-end',
      justifyContent: 'space-between',
      padding: '0 26px 6px',
      fontSize: 13,
      fontFamily: 'var(--font-mono)',
      color: 'var(--text-body)'
    }
  }, /*#__PURE__*/React.createElement("span", null, "9:41"), /*#__PURE__*/React.createElement("span", {
    style: {
      display: 'flex',
      gap: 6,
      alignItems: 'center'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      width: 16,
      height: 9,
      borderRadius: 2,
      border: '1px solid var(--text-faint)',
      display: 'inline-block'
    }
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      minHeight: 0,
      display: 'flex',
      flexDirection: 'column'
    }
  }, children)), label ? /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 11,
      letterSpacing: '0.08em',
      textTransform: 'uppercase',
      color: 'var(--text-faint)'
    }
  }, label) : null);
}
function Screen({
  children,
  style
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      minHeight: 0,
      overflowY: 'auto',
      display: 'flex',
      flexDirection: 'column',
      ...style
    }
  }, children);
}
function Body({
  children,
  style
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      padding: '0 var(--gutter-screen) 24px',
      display: 'flex',
      flexDirection: 'column',
      ...style
    }
  }, children);
}
Object.assign(window, {
  Phone,
  Screen,
  Body
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/mobile-app/phone-shell.jsx", error: String((e && e.message) || e) }); }

// ui_kits/mobile-app/subscription-screen.jsx
try { (() => {
const {
  Button,
  PlanCard,
  Icon,
  SegmentedControl
} = window.__GB;
function SubscriptionScreen({
  onContinue,
  onBack
}) {
  const [plan, setPlan] = React.useState('monthly');
  return /*#__PURE__*/React.createElement(Screen, null, /*#__PURE__*/React.createElement("div", {
    style: {
      padding: '8px var(--gutter-screen) 0',
      display: 'flex',
      justifyContent: 'flex-end'
    }
  }, /*#__PURE__*/React.createElement("button", {
    type: "button",
    onClick: onContinue,
    style: {
      background: 'none',
      border: 'none',
      color: 'var(--text-muted)',
      fontFamily: 'var(--font-body)',
      fontSize: 14,
      cursor: 'pointer'
    }
  }, "Maybe later")), /*#__PURE__*/React.createElement(Body, {
    style: {
      flex: 1,
      paddingTop: 8
    }
  }, /*#__PURE__*/React.createElement("h1", {
    style: {
      fontSize: 'var(--text-h1)',
      lineHeight: 'var(--text-h1-lh)',
      letterSpacing: 'var(--track-heading)'
    }
  }, "Pick a boot size"), /*#__PURE__*/React.createElement("p", {
    style: {
      fontSize: 15,
      color: 'var(--text-muted)',
      marginTop: 8
    }
  }, "Start free with one car. Upgrade when the glovebox fills up."), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 10,
      marginTop: 20
    }
  }, /*#__PURE__*/React.createElement(PlanCard, {
    name: "Free",
    price: "\u20AC0",
    period: "forever",
    tagline: "One car, five documents.",
    features: ['1 vehicle', '5 documents', 'Renewal reminders'],
    selected: plan === 'free',
    onClick: () => setPlan('free')
  }), /*#__PURE__*/React.createElement(PlanCard, {
    name: "Monthly",
    price: "\u20AC5",
    period: "/ month",
    tagline: "The whole household's cars.",
    features: ['Unlimited vehicles', 'Unlimited documents', 'Document scanning'],
    selected: plan === 'monthly',
    onClick: () => setPlan('monthly')
  }), /*#__PURE__*/React.createElement(PlanCard, {
    name: "Yearly",
    price: "\u20AC50",
    period: "/ year",
    badge: "Best value",
    tagline: "Two months on us.",
    features: ['Everything in Monthly', 'Service cost reports', 'Priority support'],
    selected: plan === 'yearly',
    onClick: () => setPlan('yearly')
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 'auto',
      paddingTop: 20
    }
  }, /*#__PURE__*/React.createElement(Button, {
    variant: "primary",
    size: "lg",
    fullWidth: true,
    onClick: onContinue
  }, plan === 'free' ? 'Continue for free' : plan === 'monthly' ? 'Start monthly — €5' : 'Start yearly — €50'), /*#__PURE__*/React.createElement("p", {
    style: {
      fontSize: 12,
      color: 'var(--text-faint)',
      textAlign: 'center',
      marginTop: 10
    }
  }, "Cancel any time. No handbrake turns."))));
}
Object.assign(window, {
  SubscriptionScreen
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/mobile-app/subscription-screen.jsx", error: String((e && e.message) || e) }); }

__ds_ns.AppHeader = __ds_scope.AppHeader;

__ds_ns.DocumentChip = __ds_scope.DocumentChip;

__ds_ns.EmptyState = __ds_scope.EmptyState;

__ds_ns.MaintenanceRow = __ds_scope.MaintenanceRow;

__ds_ns.ObligationRow = __ds_scope.ObligationRow;

__ds_ns.PlanCard = __ds_scope.PlanCard;

__ds_ns.SectionHeader = __ds_scope.SectionHeader;

__ds_ns.SsoButton = __ds_scope.SsoButton;

__ds_ns.StatTile = __ds_scope.StatTile;

__ds_ns.TabBar = __ds_scope.TabBar;

__ds_ns.VehicleCard = __ds_scope.VehicleCard;

__ds_ns.Badge = __ds_scope.Badge;

__ds_ns.Button = __ds_scope.Button;

__ds_ns.Card = __ds_scope.Card;

__ds_ns.Checkbox = __ds_scope.Checkbox;

__ds_ns.ICONS = __ds_scope.ICONS;

__ds_ns.ICON_NAMES = __ds_scope.ICON_NAMES;

__ds_ns.Icon = __ds_scope.Icon;

__ds_ns.IconButton = __ds_scope.IconButton;

__ds_ns.Input = __ds_scope.Input;

__ds_ns.ListRow = __ds_scope.ListRow;

__ds_ns.ProgressBar = __ds_scope.ProgressBar;

__ds_ns.SegmentedControl = __ds_scope.SegmentedControl;

__ds_ns.Select = __ds_scope.Select;

__ds_ns.Switch = __ds_scope.Switch;

})();
