/* eslint-disable @typescript-eslint/no-explicit-any */
import { IColumn } from '@fluentui/react';
import { Context } from '../../types/PcfCoreTypes';
import { SearchTypeCode } from '../../enums/SearchTypeCode';

export const openEntityFormFromLookup = (crmContext: Context, lookup: any) => {
  const entityName: string = lookup.etn;
  const entityId: string = lookup.id.guid;
  const urlBase: string = (<any>crmContext).page.getClientUrl();

  const link: string = `${urlBase}/main.aspx?etn=${entityName}&pagetype=entityrecord&id=%7B${entityId}%7D`;
  crmContext.navigation.openUrl(link);
};

export const openEntityFormFromRow = (
  crmContext: Context,
  entityLogicalName: string,
  entityId: string
) => {
  const urlBase: string = (<any>crmContext).page.getClientUrl();
  const link: string = `${urlBase}/main.aspx?etn=${entityLogicalName}&pagetype=entityrecord&id=%7B${entityId}%7D`;
  crmContext.navigation.openUrl(link);
};

export const generateLinkToEntityFormFromLookup = (
  crmContext: Context,
  item: any,
  column: IColumn
): string => {
  let link: string = '';
  const field: any = item.RawAttributes.find(
    (attribute: any) => attribute.key === column.fieldName!
  );
  if (field !== undefined) {
    const entityName: string = field.value.Value.LogicalName ?? '';
    const entityId: string = field.value.Value.Id ?? '';
    const urlBase: string = (<any>crmContext).page.getClientUrl();

    link = `${urlBase}/main.aspx?etn=${entityName}&pagetype=entityrecord&id=%7B${entityId}%7D`;
  }

  return link;
};

export const generateLookupObjectPropertyName = (
  entityLogicalName: string,
  complexEntityType?: string
): string => {
  return complexEntityType !== undefined
    ? complexEntityType + entityLogicalName + '@odata.bind'
    : entityLogicalName + '@odata.bind';
};

export const generateLookupObjectValue = (
  entityLogicalName: string,
  entityId: string
): string => {
  return `/${geEntityPluralName(entityLogicalName)}(${entityId})`;
};

export const geEntityPluralName = (entityLogicalName: string): string => {
  if (
    entityLogicalName.endsWith('s') ||
    entityLogicalName.endsWith('sh') ||
    entityLogicalName.endsWith('ch') ||
    entityLogicalName.endsWith('x') ||
    entityLogicalName.endsWith('z')
  ) {
    return entityLogicalName + 'es';
  } else if (entityLogicalName.endsWith('y')) {
    return entityLogicalName.slice(0, entityLogicalName.length) + 'ies';
  } else {
    return entityLogicalName + 's';
  }
};

export const callSearchConfigurationCustomAction = (
  context: Context,
  setSearchResult: React.Dispatch<any>
): void => {
  const relatedEntityName: string = (context as any).page.entityTypeName;
  const relatedEntityId: string = (context as any).page.entityId;

  const request: any = {
    SearchType: SearchTypeCode.Entity,
    EntityLogicalName: relatedEntityName,
    EntityId: relatedEntityId,
  };

  request.getMetadata = (): any => {
    return {
      boundParameter: null,
      parameterTypes: {
        SearchType: {
          typeName: 'Edm.Int32',
          structuralProperty: 1,
        },
        EntityLogicalName: {
          typeName: 'Edm.String',
          structuralProperty: 1,
        },
        EntityId: {
          typeName: 'Edm.String',
          structuralProperty: 1,
        },
      },
      operationType: 0,
      operationName: 'alt_FetchConfigurationManager',
    };
  };

  (context as any).webAPI
    .execute(request)
    .then((response: Response) => response.json())
    .then((json: any) => setSearchResult(json));
};
