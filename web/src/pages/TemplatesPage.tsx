import { useCallback, useEffect, useState, type ChangeEvent } from "react";
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  IconButton,
  LinearProgress,
  Stack,
  TextField,
  Typography
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import { api, getApiErrorMessage } from "../api/client";
import { useFeedback } from "../context/FeedbackContext";
import type { Template } from "../types";

interface TemplateForm {
  key: string;
  subject: string;
  body: string;
}

const emptyTemplate: TemplateForm = {
  key: "",
  subject: "",
  body: ""
};

export default function TemplatesPage() {
  const { notify } = useFeedback();
  const [templates, setTemplates] = useState<Template[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [form, setForm] = useState<TemplateForm>(emptyTemplate);
  const [deleteTarget, setDeleteTarget] = useState<Template | null>(null);
  const [deleting, setDeleting] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const { data } = await api.get<Template[]>("/templates");
      setTemplates(data);
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
    setForm(emptyTemplate);
    setEditingId(null);
  };

  const setField = (key: keyof TemplateForm) => (event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    setForm((current) => ({ ...current, [key]: event.target.value }));
  };

  const startEdit = (template: Template) => {
    setEditingId(template.id);
    setForm({
      key: template.key,
      subject: template.subject,
      body: template.body
    });
  };

  const handleSave = async () => {
    if (!form.key.trim() || !form.subject.trim() || !form.body.trim()) {
      notify("Key, subject and body are required.", "error");
      return;
    }

    setSaving(true);
    try {
      if (editingId) {
        await api.put(`/templates/${editingId}`, form);
        notify("Template updated.");
      } else {
        await api.post("/templates", form);
        notify("Template created.");
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
      await api.delete(`/templates/${deleteTarget.id}`);
      notify("Template deleted.");
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
        Templates
      </Typography>
      <Typography color="text.secondary" sx={{ mb: 3 }}>
        Create and edit your own email templates. They are private to your account and used when
        composing applications.
      </Typography>

      <Stack spacing={3}>
        {loading && <LinearProgress />}

        {!loading && templates.length === 0 && (
          <Alert severity="info">
            You have no templates yet. Create one to start composing emails.
          </Alert>
        )}

        {templates.map((template) => (
          <Card key={template.id} variant="outlined">
            <CardContent
              sx={{
                display: "flex",
                alignItems: "flex-start",
                gap: 1,
                "&:last-child": { pb: 2 }
              }}
            >
              <Box sx={{ flexGrow: 1, minWidth: 0 }}>
                <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
                  {template.key}
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                  {template.subject}
                </Typography>
              </Box>
              <IconButton aria-label={`Edit ${template.key}`} onClick={() => startEdit(template)}>
                <EditOutlinedIcon />
              </IconButton>
              <IconButton
                aria-label={`Delete ${template.key}`}
                color="error"
                onClick={() => setDeleteTarget(template)}
              >
                <DeleteOutlineIcon />
              </IconButton>
            </CardContent>
          </Card>
        ))}

        <Card variant="outlined">
          <CardContent>
            <Typography variant="h6">{editingId ? "Edit template" : "New template"}</Typography>
            <Stack spacing={2} sx={{ mt: 2 }}>
              <TextField
                label="Key"
                placeholder="Full Stack"
                value={form.key}
                onChange={setField("key")}
                fullWidth
                required
              />
              <TextField
                label="Subject"
                value={form.subject}
                onChange={setField("subject")}
                fullWidth
                required
              />
              <TextField
                label="Body"
                value={form.body}
                onChange={setField("body")}
                fullWidth
                required
                multiline
                minRows={8}
                helperText="Placeholders: {{ReceiverName}}, {{FullName}}, {{FirstName}}, {{JobTitle}}, {{CompanyName}}, {{ProfileTitle}}, {{Phone}}, {{Email}}"
              />
              <Stack direction="row" spacing={1}>
                <Button
                  variant="contained"
                  startIcon={<AddIcon />}
                  onClick={() => {
                    void handleSave();
                  }}
                  disabled={saving}
                >
                  {editingId ? "Save changes" : "Add template"}
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
      </Stack>

      <Dialog open={Boolean(deleteTarget)} onClose={() => setDeleteTarget(null)}>
        <DialogTitle>Delete template?</DialogTitle>
        <DialogContent>
          <DialogContentText>
            {deleteTarget ? `"${deleteTarget.key}" will be removed.` : ""}
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
