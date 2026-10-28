const { LeaderboardsApi } = require("@unity-services/leaderboards-1.1");
const { DataApi } = require("@unity-services/cloud-save-1.4");

const ALL_TIME = "fr_alltime";
const WEEKLY = "fr_weekly";
const HISTORY_KEY = "reward_history";
const HISTORY_SIZE = 120;
const LIMITS = { quest: 200, login: 250, bonus: 300 };
const DAY_MS = 86400000;

function parseDay(text) {
  if (!/^\d{8}$/.test(text)) {
    return NaN;
  }
  return Date.UTC(Number(text.slice(0, 4)), Number(text.slice(4, 6)) - 1, Number(text.slice(6, 8)));
}

module.exports = async ({ params, context, logger }) => {
  const { projectId, playerId } = context;
  const key = String(params.key || "");
  const points = Math.round(Number(params.points));
  const parts = key.split(":");
  const kind = parts[0];

  if (!(kind in LIMITS)) {
    throw new Error("invalid-kind");
  }
  if (kind === "quest" ? parts.length !== 3 || !/^[0-2]$/.test(parts[2]) : parts.length !== 2) {
    throw new Error("invalid-key");
  }
  const day = parseDay(parts[1]);
  const now = Date.now();
  if (!Number.isFinite(day) || day < now - 2 * DAY_MS || day > now + DAY_MS) {
    throw new Error("invalid-day");
  }
  if (!Number.isFinite(points) || points < 1 || points > LIMITS[kind]) {
    throw new Error("invalid-points");
  }

  const cloudSave = new DataApi(context);
  let history = [];
  try {
    const stored = await cloudSave.getItems(projectId, playerId, [HISTORY_KEY]);
    const item = stored.data.results.find((entry) => entry.key === HISTORY_KEY);
    history = item && Array.isArray(item.value) ? item.value : [];
  } catch (error) {
    logger.warning("reward history read failed: " + error.message);
  }
  if (history.some((entry) => entry.key === key)) {
    return { accepted: true, duplicate: true };
  }
  const sameDay = history.filter((entry) => entry.key.split(":")[1] === parts[1]);
  if (sameDay.length >= 5) {
    throw new Error("invalid-count");
  }

  const leaderboards = new LeaderboardsApi(context);
  await leaderboards.addLeaderboardPlayerScore(projectId, ALL_TIME, playerId, { score: points });
  await leaderboards.addLeaderboardPlayerScore(projectId, WEEKLY, playerId, { score: points });

  history.push({ key, at: now, points });
  await cloudSave.setItem(projectId, playerId, { key: HISTORY_KEY, value: history.slice(-HISTORY_SIZE) });
  return { accepted: true };
};
