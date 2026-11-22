import { useEffect, useState } from "react";
import {
  Alert,
  Box,
  Chip,
  LinearProgress,
  Paper,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography
} from "@mui/material";
import { api, getApiErrorMessage } from "../api/client";
import { useFeedback } from "../context/FeedbackContext";
import type { ApplicationRecord } from "../types";

function formatDate(value: string): string {
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? value : date.toLocaleString();
}

export default function HistoryPage() {
  const { notify } = useFeedback();
  const [applications, setApplications] = useState<ApplicationRecord[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;

    const load = async () => {
      try {
        const { data } = await api.get<ApplicationRecord[]>("/applications");
        if (!cancelled) {
          setApplications(data);
        }
      } catch (error) {
        if (!cancelled) {
          notify(getApiErrorMessage(error), "error");
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    };

    void load();
    return () => {
      cancelled = true;
    };
  }, [notify]);

  return (
    <Box>
      <Stack direction="row" spacing={1} alignItems="center" sx={{ mb: 1 }}>
        <Typography variant="h4">Sent applications</Typography>
        {applications.length > 0 && (
          <Chip size="small" label={applications.length} variant="outlined" />
        )}
      </Stack>
      <Typography color="text.secondary" sx={{ mb: 3 }}>
        Every application sent through JobMailer is recorded here. Open status is approximate and
        depends on the recipient loading images.
      </Typography>

      {loading ? (
        <LinearProgress />
      ) : applications.length === 0 ? (
        <Alert severity="info">
          Nothing sent yet. Applications you send will be listed here.
        </Alert>
      ) : (
        <TableContainer component={Paper} variant="outlined">
          <Table size="small" sx={{ minWidth: 820 }}>
            <TableHead>
              <TableRow>
                <TableCell>Company</TableCell>
                <TableCell>Role</TableCell>
                <TableCell>Profile</TableCell>
                <TableCell>To</TableCell>
                <TableCell>Sent</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Opens</TableCell>
                <TableCell>Last opened</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {applications.map((application) => (
                <TableRow key={application.id} hover>
                  <TableCell>{application.company ?? "—"}</TableCell>
                  <TableCell>{application.jobTitle ?? application.profileTitle}</TableCell>
                  <TableCell>{application.profileTitle}</TableCell>
                  <TableCell>{application.toEmail}</TableCell>
                  <TableCell>{formatDate(application.sentAt)}</TableCell>
                  <TableCell>
                    <Chip
                      label={application.opened ? "Opened" : "Not opened"}
                      color={application.opened ? "success" : "default"}
                      size="small"
                      variant={application.opened ? "filled" : "outlined"}
                    />
                  </TableCell>
                  <TableCell>{application.openCount}</TableCell>
                  <TableCell>
                    {application.lastOpenedAt ? formatDate(application.lastOpenedAt) : "—"}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Box>
  );
}
