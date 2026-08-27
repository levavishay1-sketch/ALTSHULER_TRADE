import { IColumn } from "@fluentui/react/lib/DetailsList";
import { ServiceProvider } from "pcf-react";

export type ControlType =
  | 'string'
  | 'number'
  | 'lookup'
  | 'boolean'
  | 'date'
  | 'optionset'
  | 'multioptionset'
  | 'checkbox'
  | 'datetime';

  export interface IFilter {
    entityLogicalName: string;
    fetchXml: string;
  }
  export interface IControlValue {
    [key: string]:
    | null
    | boolean
    | string
    | string[]
    | number
    | number[]
    | ComponentFramework.LookupValue[]
    | IGrid
  }

  export interface IGrid{
    [key: string]: any
  }

  export interface IControl {
    logicalname: string;
    label: string;
    type: ControlType;
    value: any
    values: any
  }

  export interface IForm {
    [key: string]: IControl[];
   // controls: IControl[];
  }

  export interface JsonParserComponent {
    serviceProvider: ServiceProvider;
  }
  
  export interface IDetailsList {
    columns: IColumn[];
    items: IItem[];
    isCompactMode: boolean;
  }
  
  export interface IItem {
    name: string;
    value: any;
  }
  
