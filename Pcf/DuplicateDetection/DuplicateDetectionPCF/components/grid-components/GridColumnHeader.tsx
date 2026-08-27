/* eslint-disable no-unused-vars */
/* eslint-disable react/display-name */
import * as React from 'react';

import { IColumn, IDetailsHeaderProps } from '@fluentui/react/lib/DetailsList';
import { IRenderFunction } from '@fluentui/react/lib/Utilities';
import { Sticky, StickyPositionType } from '@fluentui/react/lib/Sticky';

const headerStyles = {
  root: { paddingTop: 2 },
};

export const GridColumnHeader = (
  onColumnClick: ((columnClicked: string) => void) | undefined
): ((
  props: IDetailsHeaderProps | undefined,
  defaultRender?: IRenderFunction<IDetailsHeaderProps>
) => React.JSX.Element) => {
  const onColumnHeaderClick = (
    ev?: React.MouseEvent<HTMLElement>,
    column?: IColumn
  ): void => {
    const name = column?.fieldName ?? '';
    onColumnClick !== undefined ? onColumnClick(name) : null;
  };

  return (
    props: IDetailsHeaderProps | undefined,
    defaultRender?: IRenderFunction<IDetailsHeaderProps>
  ) =>
    props && defaultRender ? (
      <Sticky stickyPosition={StickyPositionType.Header} isScrollSynced={true}>
        {defaultRender!({
          ...props!,
          onColumnClick: onColumnHeaderClick,
          styles: headerStyles,
        })}
      </Sticky>
    ) : (
      <></>
    );
};
