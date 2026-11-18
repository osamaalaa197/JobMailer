import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Stack,
  Typography
} from "@mui/material";
import GoogleIcon from "@mui/icons-material/Google";
import MarkEmailReadIcon from "@mui/icons-material/MarkEmailRead";
import { API_BASE_URL } from "../api/client";
import { useAuth } from "../context/AuthContext";

export default function LoginPage() {
  const { authError } = useAuth();

  return (
    <Box
      sx={{
        minHeight: "100vh",
        display: "grid",
        placeItems: "center",
        px: 2,
        background:
          "radial-gradient(circle at 20% 20%, rgba(15, 92, 77, 0.12), transparent 45%), radial-gradient(circle at 80% 80%, rgba(180, 83, 9, 0.1), transparent 40%), #f7f6f2"
      }}
    >
      <Card sx={{ maxWidth: 520, width: "100%", boxShadow: 6 }}>
        <CardContent sx={{ p: { xs: 3, sm: 5 } }}>
          <Stack spacing={2.5}>
            <Box
              sx={{
                width: 56,
                height: 56,
                borderRadius: 2,
                bgcolor: "primary.main",
                color: "primary.contrastText",
                display: "grid",
                placeItems: "center"
              }}
            >
              <MarkEmailReadIcon fontSize="large" />
            </Box>

            <Box>
              <Typography variant="h4" gutterBottom>
                JobMailer
              </Typography>
              <Typography color="text.secondary">
                Send job applications from your own Gmail, with the right CV attached.
              </Typography>
            </Box>

            <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
              <Chip size="small" label="Google OAuth 2.0" variant="outlined" />
              <Chip size="small" label="Gmail API" variant="outlined" />
              <Chip size="small" label="CV attachments" variant="outlined" />
            </Stack>

            {authError && <Alert severity="error">{authError}</Alert>}

            <Button
              variant="contained"
              size="large"
              startIcon={<GoogleIcon />}
              onClick={() => {
                window.location.assign(`${API_BASE_URL}/auth/google/start`);
              }}
            >
              Sign in with Google
            </Button>

            <Typography variant="caption" color="text.secondary">
              JobMailer requests the gmail.send scope only. Your Google password is never shared with
              this application, and your tokens are encrypted at rest.
            </Typography>
          </Stack>
        </CardContent>
      </Card>
    </Box>
  );
}
