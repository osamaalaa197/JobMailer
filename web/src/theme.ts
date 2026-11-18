import { createTheme } from "@mui/material/styles";

const headingFont = '"Bricolage Grotesque", "Inter", system-ui, sans-serif';
const bodyFont = '"Inter", system-ui, -apple-system, "Segoe UI", sans-serif';

export const theme = createTheme({
  palette: {
    mode: "light",
    primary: { main: "#0f5c4d" },
    secondary: { main: "#b45309" },
    background: { default: "#f7f6f2", paper: "#ffffff" },
    text: { primary: "#1c2430", secondary: "#667085" },
    divider: "#e3e1da"
  },
  shape: { borderRadius: 10 },
  typography: {
    fontFamily: bodyFont,
    h1: { fontFamily: headingFont, fontWeight: 700 },
    h2: { fontFamily: headingFont, fontWeight: 700 },
    h3: { fontFamily: headingFont, fontWeight: 700 },
    h4: { fontFamily: headingFont, fontWeight: 700 },
    h6: { fontFamily: headingFont, fontWeight: 700 }
  },
  components: {
    MuiButton: {
      defaultProps: { disableElevation: true },
      styleOverrides: { root: { textTransform: "none", fontWeight: 600 } }
    },
    MuiTab: {
      styleOverrides: { root: { textTransform: "none", fontWeight: 600 } }
    },
    MuiPaper: {
      styleOverrides: { root: { backgroundImage: "none" } }
    },
    MuiTextField: {
      defaultProps: { size: "small" }
    }
  }
});
