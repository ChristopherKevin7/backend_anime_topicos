import { useCallback, useRef, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import ForceGraph2D from 'react-force-graph-2d';
import { NODE_COLORS } from '../utils/graphUtils';

function nodeRoute(node) {
  if (node.type === 'ANIME') return `/anime/${encodeURIComponent(node.name)}`;
  if (node.type === 'CHARACTER') return `/character/${encodeURIComponent(node.name)}`;
  return null;
}

function GraphLegend() {
  const entries = Object.entries(NODE_COLORS).filter(([k]) => k !== 'DEFAULT');
  return (
    <div style={{ display: 'flex', flexWrap: 'wrap', gap: 8, padding: '6px 12px', fontSize: 11 }}>
      {entries.map(([type, color]) => (
        <span key={type} style={{ display: 'flex', alignItems: 'center', gap: 4 }}>
          <span style={{
            width: 10, height: 10, borderRadius: '50%',
            background: color, display: 'inline-block', flexShrink: 0,
          }} />
          {type.replace('_', ' ')}
        </span>
      ))}
    </div>
  );
}

export default function AnimeGraph({ graphData }) {
  const navigate = useNavigate();
  const fgRef = useRef();

  useEffect(() => {
    const fg = fgRef.current;
    if (!fg) return;
    fg.d3Force('charge').strength(-400);
    fg.d3Force('link').distance(120);
    const timeout = setTimeout(() => {
      fg.zoomToFit(400, 40);
    }, 600);
    return () => clearTimeout(timeout);
  }, [graphData]);

  const paintNode = useCallback((node, ctx, globalScale) => {
    const radius = node.isCenter ? 10 : 6;
    const color = NODE_COLORS[node.type] ?? NODE_COLORS.DEFAULT;

    ctx.beginPath();
    ctx.arc(node.x, node.y, radius, 0, 2 * Math.PI);
    ctx.fillStyle = color;
    ctx.fill();

    if (node.isCenter) {
      ctx.beginPath();
      ctx.arc(node.x, node.y, radius + 3, 0, 2 * Math.PI);
      ctx.strokeStyle = color;
      ctx.lineWidth = 2 / globalScale;
      ctx.stroke();
    }

    if (globalScale >= 0.6) {
      const label = node.name.length > 22 ? node.name.slice(0, 20) + '…' : node.name;
      const fontSize = Math.max(8, 11 / globalScale);
      ctx.font = `${fontSize}px system-ui, sans-serif`;
      ctx.fillStyle = '#e2e8f0';
      ctx.textAlign = 'center';
      ctx.fillText(label, node.x, node.y + radius + fontSize);
    }
  }, []);

  const handleNodeClick = useCallback((node) => {
    const route = nodeRoute(node);
    if (route) navigate(route);
  }, [navigate]);

  const getNodeLabel = useCallback(
    (node) => `${node.name} [${node.type}]`,
    []
  );

  const getLinkLabel = useCallback((link) => link.label ?? '', []);

  return (
    <div style={{
      border: '1px solid var(--border)',
      borderRadius: 8,
      overflow: 'hidden',
      background: 'var(--bg-card)',
    }}>
      <GraphLegend />
      <ForceGraph2D
        ref={fgRef}
        graphData={graphData}
        nodeId="id"
        nodeLabel={getNodeLabel}
        nodeCanvasObject={paintNode}
        nodeCanvasObjectMode={() => 'replace'}
        linkLabel={getLinkLabel}
        linkDirectionalArrowLength={4}
        linkDirectionalArrowRelPos={1}
        linkCurvature={0.2}
        onNodeClick={handleNodeClick}
        cooldownTime={2000}
        d3AlphaDecay={0.03}
        d3VelocityDecay={0.4}
        width={window.innerWidth > 900 ? 860 : window.innerWidth - 40}
        height={500}
      />
    </div>
  );
}
