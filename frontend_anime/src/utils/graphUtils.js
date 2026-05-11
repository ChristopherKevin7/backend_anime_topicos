export const NODE_COLORS = {
  ANIME:       '#6366f1',
  CHARACTER:   '#22c55e',
  VOICE_ACTOR: '#f59e0b',
  STUDIO:      '#3b82f6',
  GENRE:       '#ec4899',
  STAFF:       '#14b8a6',
  DEFAULT:     '#9ca3af',
};

export function buildGraphData(centerName, centerType, items) {
  const nodeMap = new Map();

  nodeMap.set(centerName, {
    id: centerName,
    name: centerName,
    type: centerType,
    isCenter: true,
  });

  for (const item of items) {
    if (!nodeMap.has(item.name)) {
      nodeMap.set(item.name, {
        id: item.name,
        name: item.name,
        type: item.type,
        isCenter: false,
      });
    }
  }

  const links = items.map((item, i) => ({
    id: `${centerName}-${item.name}-${i}`,
    source: centerName,
    target: item.name,
    label: item.relationshipRole
      ? `${item.relationshipType} (${item.relationshipRole})`
      : item.relationshipType,
  }));

  return { nodes: Array.from(nodeMap.values()), links };
}

// Builds a linear chain graph from PathNodeDto[] ({ name, type }[])
export function buildPathGraphData(pathNodes) {
  const nodes = pathNodes.map((node, i) => ({
    id: node.name,
    name: node.name,
    type: node.type,
    isCenter: i === 0 || i === pathNodes.length - 1,
  }));

  const links = pathNodes.slice(0, -1).map((node, i) => ({
    id: `path-${i}`,
    source: node.name,
    target: pathNodes[i + 1].name,
    label: '',
  }));

  return { nodes, links };
}
