import { IInputs } from '../generated/ManifestTypes';

export type Context = ComponentFramework.Context<IInputs>;
export type DataSet = ComponentFramework.PropertyTypes.DataSet;
export type DataSetColumn = ComponentFramework.PropertyHelper.DataSetApi.Column;
export type ConditionExpression =
  ComponentFramework.PropertyHelper.DataSetApi.ConditionExpression;
export type SortStatus =
  ComponentFramework.PropertyHelper.DataSetApi.SortStatus;
export type Metadata = ComponentFramework.PropertyHelper.EntityMetadata;
export type Entity = ComponentFramework.WebApi.Entity;
export type RetrieveMultipleResponse =
  ComponentFramework.WebApi.RetrieveMultipleResponse;
export type LookupValue = ComponentFramework.LookupValue;
