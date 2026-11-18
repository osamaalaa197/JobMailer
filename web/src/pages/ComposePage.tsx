import { useEffect, useMemo, useState, type ChangeEvent } from "react";
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Grid,
  LinearProgress,
  MenuItem,
  Stack,
  TextField,
  Typography
} from "@mui/material";
import PreviewIcon from "@mui/icons-material/Preview";
import SendIcon from "@mui/icons-material/Send";
import { api, getApiErrorMessage } from "../api/client";
import { useFeedback } from "../context/FeedbackContext";
import type { Profile, RenderedEmail, Template } from "../types";

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

interface ComposeForm {
  profileId: number | "";
  templateId: number | "";
  company: string;
  jobTitle: string;
  to: string;
}

const emptyForm: ComposeForm = {
  profileId: "",
  templateId: "",
  company: "",
  jobTitle: "",
  to: ""
};

export default function ComposePage() {
  const { notify } = useFeedback();
  const [profiles, setProfiles] = useState<Profile[]>([]);
  const [templates, setTemplates] = useState<Template[]>([]);
  const [loading, setLoading] = useState(true);
  const [form, setForm] = useState<ComposeForm>(emptyForm);
  const [preview, setPreview] = useState<RenderedEmail | null>(null);
  const [previewing, setPreviewing] = useState(false);
  const [sending, setSending] = useState(false);

  useEffect(() => {
    let cancelled = false;

    const load = async () => {
      try {
        const [profileResponse, templateResponse] = await Promise.all([
          api.get<Profile[]>("/profiles"),
          api.get<Template[]>("/templates")
        ]);

        if (!cancelled) {
          setProfiles(profileResponse.data);
          setTemplates(templateResponse.data);
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

  const selectedProfile = useMemo(
    () => profiles.find((profile) => profile.id === form.profileId) ?? null,
    [profiles, form.profileId]
  );

  const emailIsValid = EMAIL_PATTERN.test(form.to.trim());

  const handleProfileChange = (event: ChangeEvent<HTMLInputElement>) => {
    const raw = event.target.value;
    const profileId = raw === "" ? "" : Number(raw);
    const profile = profiles.find((item) => item.id === profileId);
    const matchingTemplate = profile
      ? templates.find((template) => profile.title.toLowerCase().includes(template.key.toLowerCase()))
      : undefined;

    setForm((current) => ({
      ...current,
      profileId,
      templateId: matchingTemplate?.id ?? current.templateId
    }));
    setPreview(null);
  };

  const handleTemplateChange = (event: ChangeEvent<HTMLInputElement>) => {
    const raw = event.target.value;
    setForm((current) => ({ ...current, templateId: raw === "" ? "" : Number(raw) }));
    setPreview(null);
  };

  const handleCompanyChange = (event: ChangeEvent<HTMLInputElement>) => {
    setForm((current) => ({ ...current, company: event.target.value }));
    setPreview(null);
  };

  const handleJobTitleChange = (event: ChangeEvent<HTMLInputElement>) => {
    setForm((current) => ({ ...current, jobTitle: event.target.value }));
    setPreview(null);
  };

  const handlePreview = async () => {
    if (form.profileId === "" || form.templateId === "") {
      notify("Choose a profile and a template first.", "error");
      return;
    }

    setPreviewing(true);
    try {
      const { data } = await api.post<RenderedEmail>("/email/render", {
        profileId: form.profileId,
        templateId: form.templateId,
        company: form.company,
        jobTitle: form.jobTitle
      });
      setPreview(data);
    } catch (error) {
      notify(getApiErrorMessage(error), "error");
    } finally {
      setPreviewing(false);
    }
  };

  const handleSend = async () => {
    if (!preview || form.profileId === "" || form.templateId === "") {
      return;
    }

    if (!emailIsValid) {
      notify("Enter a valid recipient email.", "error");
      return;
    }

    if (!preview.subject.trim() || !preview.body.trim()) {
      notify("Subject and message cannot be empty.", "error");
      return;
    }

    setSending(true);
    try {
      await api.post("/email/send", {
        profileId: form.profileId,
        to: form.to.trim(),
        subject: preview.subject,
        body: preview.body,
        company: form.company,
        jobTitle: form.jobTitle
      });
      notify("Application sent.");
      setPreview(null);
      setForm((current) => ({ ...current, company: "", jobTitle: "", to: "" }));
    } catch (error) {
      notify(getApiErrorMessage(error), "error");
    } finally {
      setSending(false);
    }
  };

  const canPreview = form.profileId !== "" && form.templateId !== "";

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        New application
      </Typography>
      <Typography color="text.secondary" sx={{ mb: 3 }}>
        Pick a profile and template, preview the message, then send it directly through Gmail.
      </Typography>

      {loading && <LinearProgress sx={{ mb: 2 }} />}

      {!loading && profiles.length === 0 && (
        <Alert severity="warning" sx={{ mb: 2 }}>
          You need at least one profile before you can send an application. Create one on the Profiles
          tab.
        </Alert>
      )}

      <Card variant="outlined">
        <CardContent>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <TextField
                select
                fullWidth
                label="Profile"
                value={form.profileId}
                onChange={handleProfileChange}
                disabled={loading || profiles.length === 0}
              >
                <MenuItem value="">Choose a profile</MenuItem>
                {profiles.map((profile) => (
                  <MenuItem key={profile.id} value={profile.id}>
                    {profile.title}
                  </MenuItem>
                ))}
              </TextField>
            </Grid>

            <Grid item xs={12} sm={6}>
              <TextField
                select
                fullWidth
                label="Template"
                value={form.templateId}
                onChange={handleTemplateChange}
                disabled={loading || templates.length === 0}
              >
                <MenuItem value="">Choose a template</MenuItem>
                {templates.map((template) => (
                  <MenuItem key={template.id} value={template.id}>
                    {template.key}
                  </MenuItem>
                ))}
              </TextField>
            </Grid>

            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Company name"
                value={form.company}
                onChange={handleCompanyChange}
              />
            </Grid>

            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Job title"
                helperText="Defaults to the profile title"
                value={form.jobTitle}
                onChange={handleJobTitleChange}
              />
            </Grid>

            <Grid item xs={12}>
              <TextField
                fullWidth
                type="email"
                label="Send to"
                placeholder="hiring@company.com"
                value={form.to}
                onChange={(event) => {
                  setForm((current) => ({ ...current, to: event.target.value }));
                }}
                error={form.to.length > 0 && !emailIsValid}
                helperText={form.to.length > 0 && !emailIsValid ? "Enter a valid email address." : " "}
              />
            </Grid>
          </Grid>

          {selectedProfile && !selectedProfile.hasCv && (
            <Alert severity="info" sx={{ mt: 1 }}>
              This profile has no CV attached. The application will be sent without an attachment.
            </Alert>
          )}

          <Stack direction="row" spacing={1} sx={{ mt: 2 }}>
            <Button
              variant="outlined"
              startIcon={<PreviewIcon />}
              onClick={() => {
                void handlePreview();
              }}
              disabled={!canPreview || previewing}
            >
              {previewing ? "Rendering..." : "Preview email"}
            </Button>
          </Stack>
        </CardContent>
      </Card>

      {preview && (
        <Card variant="outlined" sx={{ mt: 3 }}>
          <CardContent>
            <Typography variant="h6">Preview</Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              Edit the subject or message before sending.
            </Typography>

            <Stack spacing={2}>
              <TextField
                fullWidth
                label="Subject"
                value={preview.subject}
                onChange={(event) => setPreview({ ...preview, subject: event.target.value })}
              />
              <TextField
                fullWidth
                multiline
                minRows={12}
                label="Message"
                value={preview.body}
                onChange={(event) => setPreview({ ...preview, body: event.target.value })}
              />

              <Stack direction="row" spacing={1}>
                <Button
                  variant="contained"
                  startIcon={<SendIcon />}
                  onClick={() => {
                    void handleSend();
                  }}
                  disabled={sending || !emailIsValid}
                >
                  {sending ? "Sending..." : "Send with CV"}
                </Button>
                <Button
                  onClick={() => {
                    setPreview(null);
                  }}
                  disabled={sending}
                >
                  Discard
                </Button>
              </Stack>
            </Stack>
          </CardContent>
        </Card>
      )}
    </Box>
  );
}
