import { useState, type SyntheticEvent } from "react";
import {
  AppBar,
  Box,
  Button,
  CircularProgress,
  Container,
  Tab,
  Tabs,
  Toolbar,
  Typography
} from "@mui/material";
import LogoutIcon from "@mui/icons-material/Logout";
import { useAuth } from "./context/AuthContext";
import LoginPage from "./pages/LoginPage";
import ComposePage from "./pages/ComposePage";
import ProfilesPage from "./pages/ProfilesPage";
import TemplatesPage from "./pages/TemplatesPage";
import HistoryPage from "./pages/HistoryPage";

const TAB_NAMES = ["Compose", "Profiles", "Templates", "History"] as const;
type TabName = (typeof TAB_NAMES)[number];

export default function App() {
  const { user, initializing, logout } = useAuth();
  const [tab, setTab] = useState<TabName>("Compose");

  if (initializing) {
    return (
      <Box sx={{ display: "grid", placeItems: "center", minHeight: "100vh" }}>
        <CircularProgress />
      </Box>
    );
  }

  if (!user) {
    return <LoginPage />;
  }

  const handleTabChange = (_event: SyntheticEvent, value: TabName) => {
    setTab(value);
  };

  return (
    <Box sx={{ minHeight: "100vh", bgcolor: "background.default" }}>
      <AppBar
        position="sticky"
        color="inherit"
        elevation={0}
        sx={{ borderBottom: 1, borderColor: "divider", bgcolor: "background.paper" }}
      >
        <Toolbar sx={{ gap: 2 }}>
          <Typography variant="h6" sx={{ flexGrow: 1 }}>
            JobMailer
          </Typography>
          <Typography
            variant="body2"
            color="text.secondary"
            sx={{ display: { xs: "none", sm: "block" } }}
          >
            {user.email}
          </Typography>
          <Button
            color="inherit"
            startIcon={<LogoutIcon />}
            onClick={() => {
              void logout();
            }}
          >
            Sign out
          </Button>
        </Toolbar>
      </AppBar>

      <Container maxWidth="md" sx={{ py: 3 }}>
        <Tabs
          value={tab}
          onChange={handleTabChange}
          variant="scrollable"
          allowScrollButtonsMobile
          sx={{ mb: 3, borderBottom: 1, borderColor: "divider" }}
        >
           <Tab label="Compose" value="Compose" />
           <Tab label="Profiles" value="Profiles" />
           <Tab label="Templates" value="Templates" />
           <Tab label="History" value="History" />
        </Tabs>

        {tab === "Compose" && <ComposePage />}
        {tab === "Profiles" && <ProfilesPage />}
        {tab === "Templates" && <TemplatesPage />}
        {tab === "History" && <HistoryPage />}
      </Container>
    </Box>
  );
}
