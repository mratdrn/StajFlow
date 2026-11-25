import { useEffect, useState } from "react";

export default function usePollingQuery(load, shouldPoll) {
  const [attempt, setAttempt] = useState(0);
  const [response, setResponse] = useState(null);

  useEffect(() => {
    const controller = new AbortController();
    let timer;

    async function refresh() {
      try {
        const value = await load(controller.signal);
        if (controller.signal.aborted) return;
        setResponse({ load, attempt, value, failed: false });
        if (shouldPoll(value)) timer = window.setTimeout(poll, 5000);
      } catch {
        if (!controller.signal.aborted) setResponse({ load, attempt, failed: true });
      }
    }

    function poll() {
      if (document.hidden) timer = window.setTimeout(poll, 5000);
      else refresh();
    }

    refresh();
    return () => {
      controller.abort();
      window.clearTimeout(timer);
    };
  }, [load, shouldPoll, attempt]);

  const current = response?.load === load && response.attempt === attempt ? response : null;
  return { value: current?.value, isLoading: current === null, failed: current?.failed, reload: () => setAttempt((value) => value + 1) };
}
