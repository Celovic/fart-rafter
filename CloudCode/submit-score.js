const { LeaderboardsApi } = require("@unity-services/leaderboards-1.1");
const { DataApi } = require("@unity-services/cloud-save-1.4");

const ALL_TIME = "fr_alltime";
const WEEKLY = "fr_weekly";
const HISTORY_KEY = "score_history";
const ADVENTURE_FACTORS = [1, 1.4, 2, 4];
const PRACTICE_FACTOR = 0.5;
const BASE_MAX = 100;
const CREW_MAX = 1.15;
const MIN_PAR_SHARE = 0.35;
const MIN_GAP_MS = 20000;
const HOURLY_LIMIT = 30;
const HISTORY_SIZE = 60;

module.exports = async ({ params, context, logger }) => {
  const { projectId, playerId } = context;
  const score = Math.round(Number(params.score));
  const elapsed = Number(params.elapsed);
  const par = Number(params.par);
  const humans = Math.round(Number(params.humans));
  const bots = Math.round(Number(params.bots));
  const difficulty = Math.round(Number(params.difficulty));
  const adventure = params.mode === "adventure";

  if (!adventure && params.mode !== "practice") {
    throw new Error("invalid-mode");
  }
  if (adventure && (difficulty < 0 || difficulty >= ADVENTURE_FACTORS.length)) {
    throw new Error("invalid-difficulty");
  }
  if (humans < 1 || humans > 4 || bots < 0 || bots > 3 || humans + bots > 4) {
    throw new Error("invalid-crew");
  }
  const factor = adventure ? ADVENTURE_FACTORS[difficulty] : PRACTICE_FACTOR;
  const cap = Math.ceil(BASE_MAX * factor * CREW_MAX);
  if (!Number.isFinite(score) || score < 1 || score > cap) {
    throw new Error("invalid-score");
  }
  if (!Number.isFinite(elapsed) || !Number.isFinite(par) || par <= 0 || elapsed <= 0 || elapsed > 7200 || elapsed < par * MIN_PAR_SHARE) {
    throw new Error("invalid-time");
  }

  const cloudSave = new DataApi(context);
  let history = [];
  try {
    const stored = await cloudSave.getItems(projectId, playerId, [HISTORY_KEY]);
    const item = stored.data.results.find((entry) => entry.key === HISTORY_KEY);
    history = item && Array.isArray(item.value) ? item.value : [];
  } catch (error) {
    logger.warning("history read failed: " + error.message);
  }

  if (history.some((entry) => entry.id === params.submissionId)) {
    return { accepted: true, duplicate: true };
  }
  const now = Date.now();
  const recent = history.filter((entry) => now - entry.at < 3600000);
  if (recent.length >= HOURLY_LIMIT) {
    throw new Error("rate-limit");
  }
  if (recent.length > 0 && now - Math.max(...recent.map((entry) => entry.at)) < MIN_GAP_MS) {
    throw new Error("too-fast");
  }

  const leaderboards = new LeaderboardsApi(context);
  await leaderboards.addLeaderboardPlayerScore(projectId, ALL_TIME, playerId, { score });
  await leaderboards.addLeaderboardPlayerScore(projectId, WEEKLY, playerId, { score });

  history.push({ id: params.submissionId, at: now, score });
  await cloudSave.setItem(projectId, playerId, { key: HISTORY_KEY, value: history.slice(-HISTORY_SIZE) });
  return { accepted: true };
};

module.exports.params = {
  submissionId: { type: "String", required: true },
  score: { type: "Numeric", required: true },
  level: { type: "String", required: true },
  elapsed: { type: "Numeric", required: true },
  par: { type: "Numeric", required: true },
  humans: { type: "Numeric", required: true },
  bots: { type: "Numeric", required: true },
  mode: { type: "String", required: true },
  difficulty: { type: "Numeric", required: true }
};
