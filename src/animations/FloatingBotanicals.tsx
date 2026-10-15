import type { CSSProperties } from 'react';
import './floatingBotanicals.css';

const items = [
  { kind: 'flower', lane: '12%', delay: '-3s', duration: '22s', size: 30 },
  { kind: 'leaf', lane: '28%', delay: '-14s', duration: '28s', size: 34 },
  { kind: 'petal', lane: '47%', delay: '-8s', duration: '19s', size: 22 },
  { kind: 'flower', lane: '66%', delay: '-18s', duration: '31s', size: 24 },
  { kind: 'leaf', lane: '82%', delay: '-6s', duration: '25s', size: 28 },
] as const;

function Icon({ kind }: { kind: (typeof items)[number]['kind'] }) {
  if (kind === 'leaf') return <svg viewBox="0 0 48 48"><path d="M41 7C25 8 10 16 8 34c8 5 19 3 25-4 7-8 8-23 8-23Z"/><path className="vein" d="M12 34c8-8 15-14 25-21"/></svg>;
  if (kind === 'petal') return <svg viewBox="0 0 48 48"><path d="M24 43C9 33 8 20 24 5c16 15 15 28 0 38Z"/></svg>;
  return <svg viewBox="0 0 48 48"><path d="M24 20C15 5 4 11 13 24 0 28 8 40 22 29c5 15 19 8 12-4 14-6 5-18-10-5Z"/><circle cx="24" cy="24" r="5"/></svg>;
}

export default function FloatingBotanicals() {
  return <div className="botanical-animation" aria-hidden="true">{items.map((item, index) =>
    <span className={`botanical botanical-${item.kind}`} key={index} style={{'--lane':item.lane,'--delay':item.delay,'--duration':item.duration,'--size':`${item.size}px`} as CSSProperties}><Icon kind={item.kind}/></span>
  )}</div>;
}
