import { useCallback, useEffect, useState, type ChangeEvent } from "react";
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  Grid,
  IconButton,
  LinearProgress,
  Stack,
  TextField,
  Typography
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import UploadFileIcon from "@mui/icons-material/UploadFile";
import { api, getApiErrorMessage } from "../api/client";
import { useFeedback } from "../context/FeedbackContext";
import type { Profile, ProfileInput } from "../types";

const MAX_CV_BYTES = 5 * 1024 * 1024;
const ALLOWED_EXTENSIONS = [".pdf", ".doc", ".docx"];

const emptyProfile: ProfileInput = {
  title: "",
  fullName: "",
  phone: "",
  email: "",
  cvBase64: null,
  cvFileName: null
};

function fileToBase64(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => {
      if (typeof reader.result !== "string") {
        reject(new Error("Could not read the selected file."));
        return;
      }
      const commaIndex = reader.result.indexOf(",");
      resolve(commaIndex >= 0 ? reader.result.slice(commaIndex + 1) : reader.result);
    };
    reader.onerror = () => reject(new Error("Could not read the selected file."));
    reader.readAsDataURL(file);
  });
}

export default function ProfilesPage() {
  const { notify } = useFeedback();
  const [profiles, setProfiles] = useState<Profile[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [form, setForm] = useState<ProfileInput>(emptyProfile);
  const [deleteTarget, setDeleteTarget] = useState<Profile | null>(null);
  const [deleting, setDeleting] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const { data } = await api.get<Profile[]>("/profiles");
      setProfiles(data);
    } catch (error) {
      notify(getApiErrorMessage(error), "error");
    } finally {
      setLoading(false);
    }
  }, [notify]);

  useEffect(() => {
    void load();
  }, [load]);

  const resetForm = () => {
    setForm(emptyProfile);
    setEditingId(null);
  };

  const setField = (key: keyof ProfileInput) => (event: ChangeEvent<HTMLInputElement>) => {
    setForm((current) => ({ ...current, [key]: event.target.value }));
  };

  const startEdit = (profile: Profile) => {
    setEditingId(profile.id);
    setForm({
      title: profile.title,
      fullName: profile.fullName,
      phone: profile.phone,
      email: profile.email,
      cvBase64: null,
      cvFileName: profile.cvFileName
    });
  };

  const handleFileChange = async (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    event.target.value = "";
    if (!file) {
      return;
    }

    if (file.size > MAX_CV_BYTES) {
      notify("The CV must be 5 MB or smaller.", "error");
      return;
    }

    const extension = file.name.slice(file.name.lastIndexOf(".")).toLowerCase();
    if (!ALLOWED_EXTENSIONS.includes(extension)) {
      notify("The CV must be a PDF, DOC or DOCX file.", "error");
      return;
    }

    try {
      const base64 = await fileToBase64(file);
      setForm((current) => ({ ...current, cvBase64: base64, cvFileName: file.name }));
    } catch (error) {
      notify(getApiErrorMessage(error), "error");
    }
  };

  const handleSave = async () => {
    if (!form.title.trim() || !form.fullName.trim() || !form.phone.trim() || !form.email.trim()) {
      notify("Title, full name, phone and email are required.", "error");
      return;
    }

    setSaving(true);
    try {
      if (editingId) {
        await api.put(`/profiles/${editingId}`, form);
        notify("Profile updated.");
      } else {
        await api.post("/profiles", form);
        notify("Profile created.");
      }
      resetForm();
      await load();
    } catch (error) {
      notify(getApiErrorMessage(error), "error");
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    if (!deleteTarget) {
      return;
    }

    setDeleting(true);
    try {
      await api.delete(`/profiles/${deleteTarget.id}`);
      notify("Profile deleted.");
      if (editingId === deleteTarget.id) {
        resetForm();
      }
      setDeleteTarget(null);
      await load();
    } catch (error) {
      notify(getApiErrorMessage(error), "error");
    } finally {
      setDeleting(false);
    }
  };

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Your profiles
      </Typography>
      <Typography color="text.secondary" sx={{ mb: 3 }}>
        Each profile holds the contact details and CV used in your applications.
      </Typography>

      <Grid container spacing={3}>
        <Grid item xs={12} md={7}>
          {loading ? (
            <LinearProgress />
          ) : profiles.length === 0 ? (
            <Alert severity="info">No profiles yet. Create one to start applying.</Alert>
          ) : (
            <Stack spacing={1.5}>
              {profiles.map((profile) => (
                <Card key={profile.id} variant="outlined">
                  <CardContent
                    sx={{
                      display: "flex",
                      alignItems: "flex-start",
                      gap: 1,
                      "&:last-child": { pb: 2 }
                    }}
                  >
                    <Box sx={{ flexGrow: 1, minWidth: 0 }}>
                      <Stack direction="row" spacing={1} alignItems="center" flexWrap="wrap" useFlexGap>
                        <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
                          {profile.title}
                        </Typography>
                        <Chip
                          size="small"
                          variant="outlined"
                          color={profile.hasCv ? "success" : "warning"}
                          label={profile.hasCv ? profile.cvFileName ?? "CV attached" : "No CV"}
                        />
                      </Stack>
                      <Typography variant="body2" color="text.secondary">
                        {profile.fullName} · {profile.email} · {profile.phone}
                      </Typography>
                    </Box>
                    <IconButton aria-label={`Edit ${profile.title}`} onClick={() => startEdit(profile)}>
                      <EditOutlinedIcon />
                    </IconButton>
                    <IconButton
                      aria-label={`Delete ${profile.title}`}
                      color="error"
                      onClick={() => setDeleteTarget(profile)}
                    >
                      <DeleteOutlineIcon />
                    </IconButton>
                  </CardContent>
                </Card>
              ))}
            </Stack>
          )}
        </Grid>

        <Grid item xs={12} md={5}>
          <Card variant="outlined">
            <CardContent>
              <Typography variant="h6">{editingId ? "Edit profile" : "New profile"}</Typography>
              <Stack spacing={2} sx={{ mt: 2 }}>
                <TextField
                  label="Profile title"
                  placeholder="Full Stack Developer"
                  value={form.title}
                  onChange={setField("title")}
                  fullWidth
                  required
                />
                <TextField
                  label="Full name"
                  value={form.fullName}
                  onChange={setField("fullName")}
                  fullWidth
                  required
                />
                <TextField
                  label="Phone"
                  value={form.phone}
                  onChange={setField("phone")}
                  fullWidth
                  required
                />
                <TextField
                  label="Contact email"
                  type="email"
                  value={form.email}
                  onChange={setField("email")}
                  fullWidth
                  required
                />

                <Box>
                  <Button variant="outlined" component="label" startIcon={<UploadFileIcon />}>
                    Attach CV
                    <input
                      hidden
                      type="file"
                      accept=".pdf,.doc,.docx"
                      onChange={handleFileChange}
                    />
                  </Button>
                  <Typography variant="caption" color="text.secondary" display="block" sx={{ mt: 1 }}>
                    {form.cvBase64
                      ? `Selected: ${form.cvFileName}`
                      : editingId && form.cvFileName
                        ? `Current CV: ${form.cvFileName} (choose a file to replace it)`
                        : "PDF, DOC or DOCX, up to 5 MB."}
                  </Typography>
                </Box>

                <Stack direction="row" spacing={1}>
                  <Button
                    variant="contained"
                    startIcon={<AddIcon />}
                    onClick={() => {
                      void handleSave();
                    }}
                    disabled={saving}
                  >
                    {editingId ? "Save changes" : "Add profile"}
                  </Button>
                  {editingId && (
                    <Button onClick={resetForm} disabled={saving}>
                      Cancel
                    </Button>
                  )}
                </Stack>
              </Stack>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      <Dialog open={Boolean(deleteTarget)} onClose={() => setDeleteTarget(null)}>
        <DialogTitle>Delete profile?</DialogTitle>
        <DialogContent>
          <DialogContentText>
            {deleteTarget
              ? `"${deleteTarget.title}" will be removed. Sent application history is kept.`
              : ""}
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteTarget(null)} disabled={deleting}>
            Cancel
          </Button>
          <Button
            color="error"
            variant="contained"
            onClick={() => {
              void handleDelete();
            }}
            disabled={deleting}
          >
            Delete
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
