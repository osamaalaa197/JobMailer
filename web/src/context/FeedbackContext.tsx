import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  type ReactNode
} from "react";
import { Alert, Snackbar } from "@mui/material";

type FeedbackSeverity = "success" | "error" | "info" | "warning";

interface FeedbackContextValue {
  notify: (message: string, severity?: FeedbackSeverity) => void;
}

interface FeedbackMessage {
  message: string;
  severity: FeedbackSeverity;
}

const FeedbackContext = createContext<FeedbackContextValue | undefined>(undefined);

export function FeedbackProvider({ children }: { children: ReactNode }) {
  const [feedback, setFeedback] = useState<FeedbackMessage | null>(null);

  const notify = useCallback((message: string, severity: FeedbackSeverity = "success") => {
    setFeedback({ message, severity });
  }, []);

  const value = useMemo<FeedbackContextValue>(() => ({ notify }), [notify]);

  return (
    <FeedbackContext.Provider value={value}>
      {children}
      <Snackbar
        open={Boolean(feedback)}
        autoHideDuration={5000}
        onClose={() => setFeedback(null)}
        anchorOrigin={{ vertical: "bottom", horizontal: "center" }}
      >
        <Alert
          severity={feedback?.severity ?? "info"}
          variant="filled"
          onClose={() => setFeedback(null)}
          sx={{ width: "100%" }}
        >
          {feedback?.message}
        </Alert>
      </Snackbar>
    </FeedbackContext.Provider>
  );
}

export function useFeedback(): FeedbackContextValue {
  const context = useContext(FeedbackContext);
  if (!context) {
    throw new Error("useFeedback must be used inside a FeedbackProvider.");
  }
  return context;
}
