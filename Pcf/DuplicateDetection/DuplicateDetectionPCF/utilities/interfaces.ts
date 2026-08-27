import { Context, Entity } from '../types/PcfCoreTypes';

export interface IGridProps {
  context: Context;
  containerWidth: number;
}

export interface IGridSectionProps {
  context: Context;
  relatedRecords: IRelatedRecords[];
  recordsCount: number;
  allocatedWidth: number;
  targetEntity: string;
}

export interface IRelatedRecords {
  logicalName: string;
  displayName: string;
  recordsList: Entity[];
}
