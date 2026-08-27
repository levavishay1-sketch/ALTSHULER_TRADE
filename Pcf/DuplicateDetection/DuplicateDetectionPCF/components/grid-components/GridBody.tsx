/* eslint-disable @typescript-eslint/no-explicit-any */
/* eslint-disable react/display-name */
import * as React from 'react';
import {
  ConstrainMode,
  DetailsList,
  DetailsListLayoutMode,
  IColumn,
  Label,
  ProgressIndicator,
  SelectionMode,
  Stack,
} from '@fluentui/react';
import { IGridProps } from '../../utilities/interfaces';
import { GridRow } from './GridRow';
import {
  callSearchConfigurationCustomAction,
  openEntityFormFromRow,
} from '../../utilities/common/CrmUtils';
import { generateItemsFromSearch } from '../../utilities/common/PcfDetailListUtils';
import {
  parseColumnsFromReturnOject,
  parseReturnOjectFromSearch,
} from '../../utilities/common/CommonUtils';

const gridStyles = {
  root: {
    height: '100%',
    width: '100%',
  },
};

const progessBarStyles = {
  root: {
    height: '100%',
    width: '100%',
  },
};

export const GridBody = React.memo(
  ({ context, containerWidth }: IGridProps) => {
    const [isLoaded, setIsLoaded] = React.useState<boolean>(false);
    const [hasSourceEntityId, setHasSourceEntityId] =
      React.useState<boolean>(false);
    const [key, setKey] = React.useState<string>('1');

    const [searchResult, setSearchResult] = React.useState<any>(undefined);
    const [returnObject, setReturnObject] = React.useState<any>(undefined);
    const [itemGroups, setItemGroups] = React.useState<any>(undefined);

    const [columns, setColumns] = React.useState<IColumn[]>([]);
    const [items, setItems] = React.useState<any[]>([]);

    React.useEffect(() => {
      if (
        (context as any).page.entityId !== null &&
        (context as any).page.entityId !== undefined
      ) {
        setHasSourceEntityId(true);
      }
    }, []);

    React.useEffect(() => {
      if (hasSourceEntityId) {
        callSearchConfigurationCustomAction(context, setSearchResult);
      }
    }, [hasSourceEntityId]);

    React.useEffect(() => {
      if (searchResult !== undefined && searchResult.IsSuccess) {
        parseReturnOjectFromSearch(searchResult.ReturnObject, setReturnObject);
      }
    }, [searchResult]);

    React.useEffect(() => {
      if (returnObject !== undefined && containerWidth !== -1) {
        parseColumnsFromReturnOject(
          returnObject.Columns,
          setColumns,
          containerWidth
        );
        setItemGroups(returnObject.ItemGroups);
        setKey(containerWidth.toString());
      }
    }, [returnObject, containerWidth]);

    React.useEffect(() => {
      if (itemGroups !== undefined) {
        setItems(generateItemsFromSearch(itemGroups, columns));
        setIsLoaded(true);
      }
    }, [itemGroups]);

    return hasSourceEntityId ? (
      isLoaded ? (
        <Stack styles={gridStyles}>
          <DetailsList
            key={key}
            items={items}
            columns={columns}
            layoutMode={DetailsListLayoutMode.justified}
            // constrainMode={ConstrainMode.unconstrained}
            selectionMode={SelectionMode.none}
            onItemInvoked={(item?: any, index?: number, ev?: Event) =>
              openEntityFormFromRow(
                context,
                item.EntitySchemaName,
                item.EntityId
              )
            }
            onRenderRow={GridRow(context)}
          ></DetailsList>
        </Stack>
      ) : (
        <ProgressIndicator
          label='מבצע חיפוש, אנא המתן...'
          styles={progessBarStyles}
        ></ProgressIndicator>
      )
    ) : (
      <Label>לצפייה בפרטים - יש לבצע שמירה ולרענן את המסך</Label>
    );
  }
);
