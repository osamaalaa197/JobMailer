import { useEffect, useState, type ReactNode } from "react";
import {
  Alert,
  Box,
  Card,
  CardContent,
  Chip,
  Grid,
  LinearProgress,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  ToggleButton,
  ToggleButtonGroup,
  Typography
} from "@mui/material";
import MailOutlineOutlinedIcon from "@mui/icons-material/MailOutlineOutlined";
import MarkEmailReadOutlinedIcon from "@mui/icons-material/MarkEmailReadOutlined";
import SendOutlinedIcon from "@mui/icons-material/SendOutlined";
import VisibilityOutlinedIcon from "@mui/icons-material/VisibilityOutlined";
import { api, getApiErrorMessage } from "../api/client";
import { useFeedback } from "../context/FeedbackContext";
import type { ApplicationRecord, DashboardDay, DashboardSummary } from "../types";

function formatDay(value: string): string {
  const date = new Date(value);
  return Number.isNaN(date.getTime())
    ? value
    : date.toLocaleDateString(undefined, { month: "short", day: "numeric", timeZone: "UTC" });
}

function formatDateTime(value: string): string {
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? value : date.toLocaleString();
}

function Legend({ color, label }: { color: string; label: string }) {
  return (
    <Stack direction="row" spacing={0.75} alignItems="center">
      <Box sx={{ width: 10, height: 10, bgcolor: color, borderRadius: 0.5 }} />
      <Typography variant="caption" color="text.secondary">
        {label}
      </Typography>
    </Stack>
  );
}

function ActivityChart({ data }: { data: DashboardDay[] }) {
  const max = Math.max(1, ...data.map((day) => Math.max(day.sent, day.opened)));

  return (
    <Box>
      <Stack direction="row" spacing={2} sx={{ mb: 2 }}>
        <Legend color="primary.main" label="Sent" />
        <Legend color="success.main" label="Opened" />
      </Stack>

      <Box
        sx={{
          display: "flex",
          alignItems: "flex-end",
          gap: "2px",
          height: 180,
          borderBottom: 1,
          borderColor: "divider",
          px: 0.5
        }}
      >
        {data.map((day) => (
          <Box
            key={day.date}
            title={`${formatDay(day.date)} — sent ${day.sent}, opened ${day.opened}`}
            sx={{
              flex: 1,
              height: "100%",
              display: "flex",
              alignItems: "flex-end",
              gap: "1px"
            }}
          >
            <Box
              sx={{
                flex: 1,
                height: `${(day.sent / max) * 100}%`,
                minHeight: day.sent > 0 ? 3 : 0,
                bgcolor: "primary.main",
                opacity: 0.65,
                borderRadius: "2px 2px 0 0"
              }}
            />
            <Box
              sx={{
                flex: 1,
                height: `${(day.opened / max) * 100}%`,
                minHeight: day.opened > 0 ? 3 : 0,
                bgcolor: "success.main",
                borderRadius: "2px 2px 0 0"
              }}
            />
          </Box>
        ))}
      </Box>

      <Stack direction="row" justifyContent="space-between" sx={{ mt: 1 }}>
        <Typography variant="caption" color="text.secondary">
          {data.length > 0 ? formatDay(data[0].date) : ""}
        </Typography>
        <Typography variant="caption" color="text.secondary">
          {data.length > 0 ? formatDay(data[data.length - 1].date) : ""}
        </Typography>
      </Stack>
    </Box>
  );
}

function StatCard({
  icon,
  label,
  value,
  helper,
  accent
}: {
  icon: ReactNode;
  label: string;
  value: string | number;
  helper: string;
  accent: string;
}) {
  return (
    <Card variant="outlined" sx={{ height: "100%" }}>
      <CardContent>
        <Stack direction="row" justifyContent="space-between" alignItems="flex-start" spacing={1}>
          <Box>
            <Typography variant="body2" color="text.secondary" gutterBottom>
              {label}
            </Typography>
            <Typography variant="h4" sx={{ lineHeight: 1.1 }}>
              {value}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {helper}
            </Typography>
          </Box>
          <Box
            sx={{
              color: accent,
              display: "grid",
              placeItems: "center",
              width: 40,
              height: 40,
              borderRadius: 2,
              bgcolor: "action.hover",
              flexShrink: 0
            }}
          >
            {icon}
          </Box>
        </Stack>
      </CardContent>
    </Card>
  );
}

function RecentApplications({ applications }: { applications: ApplicationRecord[] }) {
  if (applications.length === 0) {
    return <Alert severity="info">Applications you send will appear here.</Alert>;
  }

  return (
    <TableContainer>
      <Table size="small" sx={{ minWidth: 620 }}>
        <TableHead>
          <TableRow>
            <TableCell>Company</TableCell>
            <TableCell>Role</TableCell>
            <TableCell>Sent</TableCell>
            <TableCell>Status</TableCell>
            <TableCell>Last opened</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {applications.map((application) => (
            <TableRow key={application.id} hover>
              <TableCell>{application.company ?? "—"}</TableCell>
              <TableCell>{application.jobTitle ?? application.profileTitle}</TableCell>
              <TableCell>{formatDateTime(application.sentAt)}</TableCell>
              <TableCell>
                <Chip
                  label={application.opened ? `Opened ${application.openCount}x` : "Not opened"}
                  color={application.opened ? "success" : "default"}
                  size="small"
                  variant={application.opened ? "filled" : "outlined"}
                />
              </TableCell>
              <TableCell>
                {application.lastOpenedAt ? formatDateTime(application.lastOpenedAt) : "—"}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}

export default function DashboardPage() {
  const { notify } = useFeedback();
  const [days, setDays] = useState(30);
  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;

    const load = async () => {
      setLoading(true);
      try {
        const { data } = await api.get<DashboardSummary>("/dashboard", { params: { days } });
        if (!cancelled) {
          setSummary(data);
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
  }, [days, notify]);

  return (
    <Box>
      <Box sx={{ mb: 3 }}>
        <Typography variant="h4">Dashboard</Typography>
        <Typography color="text.secondary">
          Track applications and see when recipients open your emails.
        </Typography>
      </Box>

      {loading && <LinearProgress sx={{ mb: 2 }} />}

      {summary && (
        <>
          <Grid container spacing={2} sx={{ mb: 3 }}>
            <Grid item xs={12} sm={6} md={3}>
              <StatCard
                icon={<SendOutlinedIcon />}
                label="Applications sent"
                value={summary.totalSent}
                helper={`${summary.opened} opened`}
                accent="primary.main"
              />
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <StatCard
                icon={<MarkEmailReadOutlinedIcon />}
                label="Opened"
                value={summary.opened}
                helper={`${summary.openRate}% open rate`}
                accent="success.main"
              />
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <StatCard
                icon={<MailOutlineOutlinedIcon />}
                label="Not opened"
                value={summary.unopened}
                helper="No open event recorded"
                accent="warning.main"
              />
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <StatCard
                icon={<VisibilityOutlinedIcon />}
                label="Total opens"
                value={summary.totalOpens}
                helper="Includes repeat opens"
                accent="secondary.main"
              />
            </Grid>
          </Grid>

          <Grid container spacing={2}>
            <Grid item xs={12} md={8}>
              <Card variant="outlined" sx={{ height: "100%" }}>
                <CardContent>
                  <Stack
                    direction={{ xs: "column", sm: "row" }}
                    justifyContent="space-between"
                    alignItems={{ xs: "flex-start", sm: "center" }}
                    spacing={1}
                    sx={{ mb: 2 }}
                  >
                    <Typography variant="h6">Activity</Typography>
                    <ToggleButtonGroup
                      size="small"
                      exclusive
                      value={days}
                      onChange={(_event, value: number | null) => {
                        if (value !== null) {
                          setDays(value);
                        }
                      }}
                    >
                      <ToggleButton value={7}>7 days</ToggleButton>
                      <ToggleButton value={30}>30 days</ToggleButton>
                      <ToggleButton value={90}>90 days</ToggleButton>
                    </ToggleButtonGroup>
                  </Stack>
                  {summary.totalSent === 0 ? (
                    <Alert severity="info">
                      Send an application to start building analytics.
                    </Alert>
                  ) : summary.dailyActivity.some((day) => day.sent > 0 || day.opened > 0) ? (
                    <ActivityChart data={summary.dailyActivity} />
                  ) : (
                    <Alert severity="info">No sends or opens in the selected range.</Alert>
                  )}
                </CardContent>
              </Card>
            </Grid>

            <Grid item xs={12} md={4}>
              <Card variant="outlined" sx={{ height: "100%" }}>
                <CardContent>
                  <Typography variant="h6" sx={{ mb: 2 }}>
                    Top companies
                  </Typography>
                  {summary.topCompanies.length === 0 ? (
                    <Typography variant="body2" color="text.secondary">
                      Company data will appear after you send applications.
                    </Typography>
                  ) : (
                    <Stack spacing={2}>
                      {summary.topCompanies.map((company) => {
                        const rate =
                          company.sent === 0
                            ? 0
                            : Math.round((company.opened / company.sent) * 100);

                        return (
                          <Box key={company.company}>
                            <Stack
                              direction="row"
                              justifyContent="space-between"
                              spacing={1}
                              sx={{ mb: 0.5 }}
                            >
                              <Typography variant="body2" sx={{ fontWeight: 600 }} noWrap>
                                {company.company}
                              </Typography>
                              <Typography variant="caption" color="text.secondary">
                                {company.opened}/{company.sent} opened
                              </Typography>
                            </Stack>
                            <LinearProgress
                              variant="determinate"
                              value={rate}
                              color={rate >= 50 ? "success" : "primary"}
                            />
                          </Box>
                        );
                      })}
                    </Stack>
                  )}
                </CardContent>
              </Card>
            </Grid>

            <Grid item xs={12}>
              <Card variant="outlined">
                <CardContent>
                  <Typography variant="h6" sx={{ mb: 2 }}>
                    Recent applications
                  </Typography>
                  <RecentApplications applications={summary.recentApplications} />
                </CardContent>
              </Card>
            </Grid>
          </Grid>
        </>
      )}
    </Box>
  );
}
